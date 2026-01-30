# Phase 7: Messaging Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Set up event-driven communication with guaranteed delivery using Full DDD approach (Domain Events -> Integration Events) with MassTransit built-in Outbox.

## Scope
- [x] Create AggregateRoot base class with domain events support
- [x] Implement Order domain events (Confirmed, Rejected, Cancelled)
- [x] Create domain event dispatcher (MediatR pipeline behavior)
- [x] Configure MassTransit CorrelationId filters
- [x] Configure MassTransit Bus Outbox in Order Service
- [x] Create domain event handlers (transform -> publish integration events)
- [x] Configure MassTransit in Product Service
- [x] Implement StockLow event publishing
- [x] Create reusable `MessagingExtensions.AddMessaging<TDbContext>()` helper

## Tasks

| # | ID | Task | Status | Dependencies |
|---|-----|------|--------|--------------|
| 1 | task-01 | AggregateRoot base class | ✅ | - |
| 2 | task-02 | Order Domain Events | ✅ | - |
| 3 | task-03 | Order Entity Domain Events | ✅ | task-01, task-02 |
| 4 | task-04 | Domain Event Dispatcher | ✅ | task-01 |
| 5 | task-05 | MassTransit CorrelationId Filters | ✅ | - |
| 6 | task-06 | MassTransit Bus Outbox (Order) | ✅ | task-05 |
| 7 | task-07 | Domain Event Handlers | ✅ | task-02, task-04, task-06 |
| 8 | task-08 | MassTransit (Product Service) | ✅ | task-05 |
| 9 | task-09 | Product StockLow Publishing | ✅ | task-08 |

## Related Specs
- -> [messaging-communication.md](../high-level-specs/messaging-communication.md)
- -> [correlation-id-flow.md](../high-level-specs/correlation-id-flow.md) (messaging filters)

---
## Implementation Summary

### Architecture
```
Domain Event (Order.Domain)
    ↓ MediatR dispatch via UnitOfWorkExecutor
DomainEventNotification<T> wrapper
    ↓ Handler in Application layer
Integration Event (EShop.Contracts)
    ↓ MassTransit IPublishEndpoint
Bus Outbox (PostgreSQL)
    ↓ Background delivery
RabbitMQ Exchange
    ↓
Consumers (Notification/Analytics)
```

### Key Components

**Shared Kernel** (`src/Common/EShop.SharedKernel/`):
- `IDomainEvent` - marker interface
- `DomainEventBase` - base record with `OccurredOn`
- `AggregateRoot` - entity base with domain events + version

**Domain Events Dispatch** (`src/Common/EShop.Common.Application/`):
- `IDomainEventDispatcher` + `MediatRDomainEventDispatcher`
- `DomainEventNotification<T>` - MediatR wrapper
- `UnitOfWorkExecutor` - dispatches events, then saves

**MassTransit Correlation** (`src/Common/EShop.Common.Infrastructure/Correlation/MassTransit/`):
- `CorrelationIdPublishFilter<T>`
- `CorrelationIdSendFilter<T>`
- `CorrelationIdConsumeFilter<T>`
- `MassTransitCorrelationExtensions.UseCorrelationIdFilters()`

**Messaging Helper** (`src/Common/EShop.Common.Infrastructure/Extensions/`):
- `MessagingExtensions.AddMessaging<TDbContext>()` - with Outbox
- `MessagingExtensions.AddMessaging()` - without Outbox (consumers only)

### Integration Events (EShop.Contracts)
- `OrderConfirmedEvent(OrderId, CustomerId, TotalAmount, CustomerEmail)`
- `OrderRejectedEvent(OrderId, CustomerId, CustomerEmail, Reason)`
- `OrderCancelledEvent(OrderId, CustomerId, CustomerEmail, Reason)`
- `StockLowEvent(ProductId, ProductName, CurrentQuantity, Threshold)`

### Service Configuration
- **Order Service**: `AddMessaging<OrderDbContext>()` with endpoint prefix "order"
- **Products Service**: `AddMessaging<ProductDbContext>()` with endpoint prefix "products"
- **Notification Service**: `AddMessaging()` (no outbox, consumer-only) with endpoint prefix "notification"

## Notes
- Using MassTransit built-in Bus Outbox instead of custom OutboxProcessor
- Full DDD approach: Domain Events dispatched via MediatR, handlers publish Integration Events
- CorrelationId propagation via MassTransit filters (publish/consume)
- Notification Service uses Inbox pattern for exactly-once processing (`IdempotentConsumer<T>`)
