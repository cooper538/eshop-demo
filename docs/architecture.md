# Architecture

Detailed documentation of EShop Demo architectural patterns and design decisions.

## Contents

1. [High-Level Overview](#high-level-overview)
2. [Clean Architecture](#clean-architecture)
3. [DDD Building Blocks](#ddd-building-blocks)
4. [CQRS with MediatR](#cqrs-with-mediatr)
5. [Communication Patterns](#communication-patterns)
6. [Reliability Patterns](#reliability-patterns)
7. [Distributed Tracing](#distributed-tracing)
8. [Trade-offs and Decisions](#trade-offs-and-decisions)

---

## High-Level Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                         API Gateway                               │
│                        (YARP Proxy)                               │
└─────────────────────────────┬────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
       ┌────────────┐  ┌────────────┐  ┌────────────┐
       │  Product   │  │   Order    │  │ Analytics  │
       │  Service   │  │  Service   │  │  Service   │
       └─────┬──────┘  └─────┬──────┘  └────────────┘
             │               │                ▲
             │    gRPC       │                │
             │◄──────────────┘                │
             │                                │
             ▼                                │
       ┌──────────┐     Integration Events    │
       │PostgreSQL│  ┌────────────────────────┘
       └──────────┘  │
                     │
              ┌──────┴──────┐
              │  RabbitMQ   │
              └──────┬──────┘
                     │
       ┌─────────────┼─────────────┐
       ▼             ▼             ▼
┌────────────┐ ┌────────────┐ ┌────────────┐
│Notification│ │ Analytics  │ │   Other    │
│  Service   │ │  Service   │ │ Consumers  │
└────────────┘ └────────────┘ └────────────┘
```

### Service Responsibilities

| Service | Domain | Key Features |
|---------|--------|--------------|
| **Product** | Catalog + Inventory | Product CRUD, stock management, gRPC server for stock checks |
| **Order** | Order Lifecycle | Order placement, state machine, gRPC client for stock reservation |
| **Notification** | Customer Communication | Email notifications triggered by domain events |
| **Analytics** | Business Intelligence | Event aggregation, metrics tracking |
| **Gateway** | Request Routing | YARP proxy, rate limiting, correlation ID injection |

---

## Clean Architecture

Each service follows Clean Architecture with four distinct layers:

```
┌─────────────────────────────────────────────┐
│                    API                       │  ← Controllers, minimal logic
├─────────────────────────────────────────────┤
│               Infrastructure                 │  ← EF Core, MassTransit, gRPC
├─────────────────────────────────────────────┤
│                Application                   │  ← Commands, Queries, Handlers
├─────────────────────────────────────────────┤
│                  Domain                      │  ← Entities, Value Objects, Events
└─────────────────────────────────────────────┘
```

### Dependency Rule

Dependencies point inward only:
- **Domain** - Zero external dependencies (pure C#)
- **Application** - References Domain only
- **Infrastructure** - References Application and Domain
- **API** - References all layers, but only for composition

### Layer Responsibilities

**Domain Layer**
- Entities with business logic
- Value Objects for concepts without identity
- Domain Events for side effects
- Domain Services for cross-aggregate logic

**Application Layer**
- Command/Query handlers (CQRS)
- DTOs for data transfer
- Validation logic
- Application services

**Infrastructure Layer**
- EF Core DbContext and configurations
- Repository implementations
- Message bus integration
- External service clients

**API Layer**
- Controllers (thin, delegation only)
- Middleware configuration
- Dependency injection setup

---

## DDD Building Blocks

### SharedKernel

The `EShop.SharedKernel` project provides base types for DDD:

```csharp
// Entity base with ID and domain events
public abstract class EntityBase<TId> : IEntity<TId>
{
    public TId Id { get; protected set; }
    private readonly List<IDomainEvent> _domainEvents = [];

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);
}

// Aggregate root marker
public abstract class AggregateRoot<TId> : EntityBase<TId>, IAggregateRoot
{
}

// Value Object base with equality
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
```

### Aggregate Design

Aggregates encapsulate business rules and maintain invariants:

```csharp
public class OrderEntity : AggregateRoot<OrderId>
{
    public EOrderStateType State { get; private set; }
    private readonly List<OrderItemEntity> _items = [];

    public void AddItem(ProductId productId, int quantity, decimal price)
    {
        // Business rule: can only add items to pending orders
        if (State != EOrderStateType.Pending)
            throw new DomainException("Cannot modify confirmed order");

        _items.Add(new OrderItemEntity(productId, quantity, price));
        RaiseDomainEvent(new OrderItemAddedEvent(Id, productId, quantity));
    }
}
```

### Domain Events

Events capture significant domain occurrences:

```csharp
public record OrderCreatedEvent(
    OrderId OrderId,
    CustomerId CustomerId,
    IReadOnlyList<OrderItemDto> Items,
    decimal TotalAmount
) : IDomainEvent;
```

Events are dispatched after entity persistence via MediatR notifications.

---

## CQRS with MediatR

Commands and queries are separated for clarity and optimization.

### Command Flow

```
Controller → Command → Handler → Domain → Repository → DB
                                    ↓
                              Domain Events → Integration Events → Message Bus
```

```csharp
// Command
public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderItemDto> Items
) : ICommand<OrderId>;

// Handler
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderId>
{
    public async Task<OrderId> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = OrderEntity.Create(new CustomerId(cmd.CustomerId), cmd.Items);
        await _dbContext.Orders.AddAsync(order, ct);
        return order.Id;
    }
}
```

### Query Flow

```
Controller → Query → Handler → DB (direct or read model)
```

```csharp
// Query
public record GetOrderQuery(Guid OrderId) : IQuery<OrderDetailDto>;

// Handler - uses projection, not loading full aggregate
public class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(GetOrderQuery query, CancellationToken ct)
    {
        return await _dbContext.Orders
            .Where(o => o.Id == new OrderId(query.OrderId))
            .Select(o => new OrderDetailDto { /* projection */ })
            .FirstOrDefaultAsync(ct);
    }
}
```

### Pipeline Behaviors

MediatR pipeline provides cross-cutting concerns:

```
Request → Logging → Validation → UnitOfWork → Handler → Response
```

| Behavior | Purpose |
|----------|---------|
| `LoggingBehavior` | Request/response logging |
| `ValidationBehavior` | FluentValidation execution |
| `UnitOfWorkBehavior` | Transaction + domain event dispatch |

**Critical:** UnitOfWork behavior must be registered last to wrap the entire operation.

---

## Communication Patterns

### Synchronous: gRPC

Used for operations requiring immediate response (stock checks):

```
Order Service                     Product Service
     │                                  │
     │ ── CheckStockRequest ──────────► │
     │                                  │ Check inventory
     │ ◄── StockAvailabilityResponse ── │
     │                                  │
     │ ── ReserveStockRequest ────────► │
     │                                  │ Decrement stock
     │ ◄── ReservationResponse ──────── │
```

**Why gRPC?**
- Strong typing via Proto contracts
- Efficient binary serialization
- Built-in streaming support
- Service discovery via Aspire

### Asynchronous: Integration Events

Used for eventual consistency and decoupling:

```
Order Service                    RabbitMQ              Notification Service
     │                              │                         │
     │ ── OrderCreatedEvent ──────► │                         │
     │                              │ ── OrderCreatedEvent ──►│
     │                              │                         │ Send email
     │                              │                         │
     │ ── OrderShippedEvent ──────► │                         │
     │                              │ ── OrderShippedEvent ──►│
     │                              │                         │ Send email
```

**Event Design: Fat Events**

Events carry all necessary data, reducing consumer queries:

```csharp
public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,      // Denormalized
    string CustomerName,       // Denormalized
    List<OrderItemDto> Items,  // Full item details
    decimal TotalAmount,
    DateTime CreatedAt
) : IIntegrationEvent;
```

---

## Reliability Patterns

### Outbox Pattern (Publisher Side)

Ensures events are published exactly once, even if the message broker fails:

```
┌─────────────────────────────────────────────────────┐
│                    Transaction                       │
│  ┌─────────────┐         ┌─────────────────────┐   │
│  │ Save Entity │         │ Save to OutboxTable │   │
│  └─────────────┘         └─────────────────────┘   │
└─────────────────────────────────────────────────────┘
                              │
                              ▼ (Background job)
                    ┌─────────────────────┐
                    │ Publish to RabbitMQ │
                    │ Mark as processed   │
                    └─────────────────────┘
```

**Implementation:** MassTransit's built-in Outbox with EF Core integration.

### Inbox Pattern (Consumer Side)

Ensures messages are processed exactly once, handling redeliveries:

```
Message Arrives
      │
      ▼
┌──────────────────┐
│ Check InboxTable │──── Already processed? ──► Acknowledge & Skip
└────────┬─────────┘
         │ New message
         ▼
┌─────────────────────────────────────────────────────┐
│                    Transaction                       │
│  ┌─────────────────┐       ┌───────────────────┐   │
│  │ Process Message │       │ Save to InboxTable │   │
│  └─────────────────┘       └───────────────────┘   │
└─────────────────────────────────────────────────────┘
```

**Implementation:** Custom `IdempotentConsumer<T>` base class.

---

## Distributed Tracing

### CorrelationId Flow

Every request gets a correlation ID that flows through all services:

```
Client Request (X-Correlation-Id: abc-123)
         │
         ▼
    ┌─────────┐
    │ Gateway │ ─── Injects if missing
    └────┬────┘
         │ X-Correlation-Id: abc-123
         ▼
    ┌─────────┐
    │  Order  │ ─── Logs with [abc-123]
    └────┬────┘
         │ gRPC metadata: correlation-id=abc-123
         ▼
    ┌─────────┐
    │ Product │ ─── Logs with [abc-123]
    └─────────┘
         │ Message header: CorrelationId=abc-123
         ▼
    ┌──────────────┐
    │ Notification │ ─── Logs with [abc-123]
    └──────────────┘
```

### Log Format

```
[14:32:15 INF] [abc-123] Processing order creation
[14:32:15 INF] [abc-123] Checking stock for product xyz
[14:32:16 INF] [abc-123] Stock reserved successfully
[14:32:16 INF] [abc-123] Order created: order-456
```

### Tracing Tool

```bash
# Trace a request across all services
./tools/e2e-test/trace-correlation.sh abc-123
```

---

## Trade-offs and Decisions

### Fat Events vs Thin Events

**Decision:** Fat events (events carry all data)

**Trade-off:**
- ✅ Consumers don't need to query back
- ✅ Better temporal decoupling
- ❌ Larger message payloads
- ❌ Data duplication

**Rationale:** Notification service shouldn't need to call Order service to get customer email.

### No Repository Pattern

**Decision:** DbContext used directly in handlers

**Trade-off:**
- ✅ Simpler code, less abstraction
- ✅ Full EF Core feature access
- ❌ Harder to unit test (need in-memory DB)
- ❌ Handlers coupled to EF Core

**Rationale:** For this demo, simplicity trumps testability. Repository would add indirection without clear benefit.

### Aggregate-per-Transaction

**Decision:** One aggregate modified per transaction

**Trade-off:**
- ✅ Clear consistency boundaries
- ✅ Better concurrency handling
- ❌ Cross-aggregate operations need eventual consistency

**Rationale:** DDD best practice. Cross-aggregate coordination via domain events.

### gRPC for Stock Checks

**Decision:** Synchronous gRPC instead of event-driven stock

**Trade-off:**
- ✅ Immediate feedback for user
- ✅ Simpler order flow
- ❌ Temporal coupling between services
- ❌ Order fails if Product service is down

**Rationale:** Stock check is part of order placement UX - user needs immediate feedback.

### Aspire for Orchestration

**Decision:** .NET Aspire instead of Docker Compose

**Trade-off:**
- ✅ Native .NET integration
- ✅ Automatic service discovery
- ✅ Built-in dashboard
- ❌ Newer technology, less community knowledge
- ❌ Requires .NET 8+ SDK

**Rationale:** Demonstrates modern .NET stack, excellent developer experience.
