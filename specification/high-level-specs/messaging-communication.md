# Messaging Communication Architecture

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Asynchronous inter-service communication |
| Technology | MassTransit + RabbitMQ |
| Pattern | Publish/Subscribe |
| Reliability | Outbox Pattern (publisher) + Inbox Pattern (consumer) |

---

## 1. Overview

Asynchronous messaging is used for event-driven communication, primarily for notifications. This provides:
- **Loose coupling** between services
- **Guaranteed delivery** via Outbox pattern
- **Idempotent processing** via Inbox pattern
- **Scalability** through message queues

```
┌─────────────┐                      ┌─────────────┐
│ Order       │ ─── Integration ───▶ │             │
│ Service     │     Events           │  RabbitMQ   │
└─────────────┘                      │             │
                                     │             │
┌─────────────┐                      │             │     ┌──────────────┐
│ Product     │ ─── Integration ───▶ │             │ ──▶ │ Notification │
│ Service     │     Events           │             │     │ Service      │
└─────────────┘                      └─────────────┘     └──────────────┘
```

---

## 2. Integration Events Catalog

| Event | Publisher | Subscribers | Description |
|-------|-----------|-------------|-------------|
| `OrderConfirmed` | Order | Notification | Order confirmed successfully |
| `OrderRejected` | Order | Notification | Order rejected (insufficient stock) |
| `OrderCancelled` | Order | Notification | Order cancelled by user |
| `StockLow` | Product | Notification | Stock below threshold |
| `StockReservationExpired` | Product | Order | Reservation TTL exceeded (cleanup) |
| `ProductCreated` | Product | – | New product in catalog |
| `ProductUpdated` | Product | – | Product details changed |

---

## 3. Event Design Philosophy

Integration events should follow these principles for **loose coupling**:

### ✅ Events SHOULD contain:

| Type | Description | Example |
|------|-------------|---------|
| **Identifiers** | IDs to fetch related data | `OrderId`, `CustomerId`, `ProductId` |
| **Immutable facts** | Data true at event time | `TotalAmount`, `Reason`, `Threshold` |
| **Audit data** | For replay/debugging | `Timestamp`, `CorrelationId` |

### ❌ Events SHOULD NOT contain:

| Type | Why | Solution |
|------|-----|----------|
| **Consumer-specific data** | Creates coupling | Consumer fetches via gRPC/HTTP |
| **Mutable entity data** | May change after event | Fetch current state when needed |
| **Derived/computed data** | Duplicates logic | Consumer computes if needed |

### Example: OrderConfirmed

The general rule is to keep events thin. However, **pragmatic exceptions** exist:

```
❌ GENERALLY BAD: Large mutable data in events
✅ GENERALLY GOOD: IDs only → consumer fetches current state
✅ PRAGMATIC EXCEPTION: Rarely-changing data needed by all consumers
```

**OrderConfirmed includes CustomerEmail because:**
- Email changes are rare (< 0.1% of orders affected)
- ALL notification consumers need it
- Sync dependency on Order Service for every notification is worse than occasional stale email
- Fallback: Consumer can still fetch if email delivery fails

This is a **conscious trade-off**, not a violation of the principle.

---

## 4. Event Definitions

### 4.1 Base Class

```csharp
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    // Note: CorrelationId is propagated via MassTransit message headers (implicit)
    // See: correlation-id-flow.md for details
}
```

### 4.2 Order Events

```csharp
public record OrderConfirmed : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;  // Pragmatic inclusion
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<OrderItemInfo> Items { get; init; } = [];  // For email template
}

public record OrderItemInfo(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public record OrderRejected : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record OrderCancelled : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
```

### 4.3 Product Events

```csharp
public record StockLow : IntegrationEvent
{
    public Guid ProductId { get; init; }
    public int CurrentQuantity { get; init; }
    public int Threshold { get; init; }
}

public record StockReservationExpired : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

public record ProductCreated : IntegrationEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

public record ProductUpdated : IntegrationEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
```

---

## 5. Outbox Pattern (Publisher Side)

### 5.1 Purpose

Guarantees that integration events are published even if the message broker is temporarily unavailable. Events are stored in the database within the same transaction as the business operation.

### 5.2 Database Schema

```sql
CREATE TABLE OutboxMessages (
    Id UUID PRIMARY KEY,
    EventType VARCHAR(255) NOT NULL,
    Payload JSONB NOT NULL,
    CorrelationId VARCHAR(100),
    CreatedAt TIMESTAMP NOT NULL,
    ProcessedAt TIMESTAMP NULL,
    Error VARCHAR(1000) NULL,
    RetryCount INT DEFAULT 0
);
```

### 5.3 Outbox Processor

```csharp
public class OutboxProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _repository.GetUnprocessedMessages(batchSize: 100);

            foreach (var message in messages)
            {
                try
                {
                    var @event = JsonSerializer.Deserialize(
                        message.Payload,
                        Type.GetType(message.EventType));
                    await _publishEndpoint.Publish(@event, stoppingToken);
                    await _repository.MarkAsProcessed(message.Id);
                }
                catch (Exception ex)
                {
                    await _repository.MarkAsFailed(message.Id, ex.Message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

### 5.4 Publishing Flow

```
1. Business operation starts transaction
2. Save domain changes to database
3. Save OutboxMessage to database (same transaction)
4. Commit transaction
5. OutboxProcessor picks up message (background)
6. Publish to RabbitMQ via MassTransit
7. Mark OutboxMessage as processed
```

---

## 6. Inbox Pattern (Consumer Side)

### 6.1 Purpose

Ensures idempotent message processing. If a message is delivered multiple times (at-least-once delivery), it will only be processed once.

### 6.2 Database Schema

```sql
CREATE TABLE ProcessedMessages (
    MessageId UUID NOT NULL,
    ConsumerType VARCHAR(255) NOT NULL,
    ProcessedAt TIMESTAMP NOT NULL,
    PRIMARY KEY (MessageId, ConsumerType)
);
```

### 6.3 Idempotent Consumer Base Class

```csharp
public abstract class IdempotentConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    private readonly InboxDbContext _dbContext;
    private readonly ILogger _logger;

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = context.MessageId ?? Guid.NewGuid();
        var consumerType = GetType().Name;

        if (await _dbContext.ProcessedMessages
            .AnyAsync(m => m.MessageId == messageId && m.ConsumerType == consumerType))
        {
            _logger.LogInformation(
                "Message {MessageId} already processed by {Consumer}",
                messageId, consumerType);
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        await ProcessMessage(context);

        _dbContext.ProcessedMessages.Add(new ProcessedMessage
        {
            MessageId = messageId,
            ConsumerType = consumerType,
            ProcessedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    protected abstract Task ProcessMessage(ConsumeContext<TMessage> context);
}
```

### 6.4 Consumer Implementation Example

Consumer uses data directly from the event (no service calls needed):

```csharp
public class OrderConfirmedConsumer : IdempotentConsumer<OrderConfirmed>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderConfirmedConsumer> _logger;

    public OrderConfirmedConsumer(
        IEmailService emailService,
        ILogger<OrderConfirmedConsumer> logger,
        InboxDbContext inboxDbContext) : base(inboxDbContext, logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ProcessMessage(ConsumeContext<OrderConfirmed> context)
    {
        var message = context.Message;

        // All data available directly from the event - no service calls needed
        var itemsHtml = string.Join("", message.Items.Select(i =>
            $"<li>{i.ProductName} x{i.Quantity} - {i.UnitPrice:N2} CZK</li>"));

        var result = await _emailService.SendAsync(new EmailMessage(
            To: message.CustomerEmail,  // Directly from event
            Subject: $"Order #{message.OrderId} confirmed",
            HtmlBody: $"""
                <h1>Thank you for your order!</h1>
                <p>Your order has been successfully confirmed.</p>
                <ul>{itemsHtml}</ul>
                <p><strong>Total amount:</strong> {message.TotalAmount:N2} CZK</p>
                """
        ));

        if (!result.Success)
        {
            _logger.LogWarning(
                "Failed to send order confirmation email for {OrderId}: {Error}",
                message.OrderId, result.ErrorMessage);
        }
    }
}
```

> **Note**: With "fat events", Notification Service has no runtime dependency on Order Service. This improves reliability - notifications work even when Order Service is temporarily unavailable.

---

## 7. Resiliency Patterns

### 7.1 Consumer Retry Configuration

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15)));

        cfg.ConfigureEndpoints(context);
    });
});
```

### 7.2 Dead Letter Queue

Failed messages after all retries are moved to a dead letter queue for manual inspection:

```csharp
cfg.ReceiveEndpoint("order-confirmed", e =>
{
    e.ConfigureConsumer<OrderConfirmedConsumer>(context);

    // Messages that fail all retries go here
    e.ConfigureDeadLetterQueueErrorTransport();
});
```

### 7.3 Failure Scenarios

| Scenario | Behavior | Recovery |
|----------|----------|----------|
| RabbitMQ unavailable (publish) | Message stays in Outbox | OutboxProcessor retries on next cycle |
| RabbitMQ unavailable (consume) | Consumer disconnects | Reconnects automatically |
| Consumer throws exception | MassTransit retry policy | 3 retries, then dead letter |
| Duplicate message delivered | Inbox pattern detects | Message ignored (idempotent) |

---

## 8. MassTransit Configuration

### 8.1 Publisher Configuration (Order/Product Service)

```csharp
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });
    });
});
```

### 8.2 Consumer Configuration (Notification Service)

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();
    x.AddConsumer<OrderRejectedConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();
    x.AddConsumer<StockLowConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15)));

        cfg.ConfigureEndpoints(context);
    });
});
```

---

## Related Documents

- [Order Service Interface](./order-service-interface.md) - Events published by Order service
- [Product Service Interface](./product-service-interface.md) - Events published by Product service
- [CorrelationId Flow](./correlation-id-flow.md) - Correlation ID propagation in messages