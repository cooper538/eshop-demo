# Task 5: Order Event Consumers

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | task-03, task-04 |

## Summary
Implement consumers for OrderConfirmed, OrderRejected, and OrderCancelled events.

## Scope
- [x] Create `OrderConfirmedConsumer` extending `IdempotentConsumer<OrderConfirmedEvent>`
  - Sends confirmation email with order ID and total amount
- [x] Create `OrderRejectedConsumer` extending `IdempotentConsumer<OrderRejectedEvent>`
  - Sends rejection email with order ID and reason
- [x] Create `OrderCancelledConsumer` extending `IdempotentConsumer<OrderCancelledEvent>`
  - Sends cancellation email with order ID and reason
- [x] Register all consumers in MassTransit configuration
- [x] Retry policy configured globally (1s, 5s, 15s intervals)

## Implementation

### OrderConfirmedConsumer
```csharp
public sealed class OrderConfirmedConsumer(
    NotificationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    ILogger<OrderConfirmedConsumer> logger
) : IdempotentConsumer<OrderConfirmedEvent>(dbContext, dateTimeProvider, logger)
{
    protected override async Task ProcessMessage(ConsumeContext<OrderConfirmedEvent> context)
    {
        var message = context.Message;
        var email = new EmailMessage(
            To: message.CustomerEmail,
            Subject: $"Order #{message.OrderId} Confirmed",
            HtmlBody: $"""
            <h1>Order Confirmed</h1>
            <p>Thank you for your order!</p>
            <p><strong>Order ID:</strong> {message.OrderId}</p>
            <p><strong>Total Amount:</strong> {message.TotalAmount:C}</p>
            """
        );
        await emailService.SendAsync(email, context.CancellationToken);
    }
}
```

### Consumer Registration
```csharp
x.AddConsumer<OrderConfirmedConsumer>();
x.AddConsumer<OrderRejectedConsumer>();
x.AddConsumer<OrderCancelledConsumer>();
```

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 6.4. Consumer Implementation Example)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 4.2. Order Events)

---
## Notes
- Uses integration events from `EShop.Contracts.IntegrationEvents.Order`
- All consumers use primary constructor syntax
- Email body uses C# raw string literals for clean HTML templates
