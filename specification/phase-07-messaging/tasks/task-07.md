# Task 07: Domain Event Handlers

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-02, task-04, task-06 |

## Summary
Create MediatR handlers that transform domain events to integration events and publish via MassTransit.

## Scope
- [x] Create `OrderConfirmedDomainEventHandler` implementing `INotificationHandler<DomainEventNotification<OrderConfirmedDomainEvent>>`
- [x] Create `OrderRejectedDomainEventHandler` implementing `INotificationHandler<DomainEventNotification<OrderRejectedDomainEvent>>`
- [x] Create `OrderCancelledDomainEventHandler` implementing `INotificationHandler<DomainEventNotification<OrderCancelledDomainEvent>>`
- [x] Each handler transforms domain event to corresponding integration event from `EShop.Contracts`
- [x] Publish integration event via `IPublishEndpoint` (MassTransit Bus Outbox stores in DB)
- [x] Use `IDateTimeProvider` for integration event timestamp
- [x] Add appropriate logging in each handler
- [x] Verify handlers are registered via MediatR assembly scanning

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 4.2: Order Events)

---
## Notes
- Location: `src/Services/Order/Order.Application/EventHandlers/`
- Handlers use `DomainEventNotification<T>` wrapper (not raw domain event)
- Integration events: `OrderConfirmedEvent`, `OrderRejectedEvent`, `OrderCancelledEvent`
- Integration events located in `EShop.Contracts.IntegrationEvents.Order`
- `IDateTimeProvider.UtcNow` used for `Timestamp` property
