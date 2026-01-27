# Task 07: Domain Event Handlers

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | :white_circle: pending |
| Dependencies | task-02, task-04, task-06 |

## Summary
Create MediatR handlers that transform domain events to integration events and publish via MassTransit.

## Scope
- [ ] Create `OrderConfirmedDomainEventHandler` implementing `INotificationHandler<OrderConfirmedDomainEvent>`
- [ ] Create `OrderRejectedDomainEventHandler` implementing `INotificationHandler<OrderRejectedDomainEvent>`
- [ ] Create `OrderCancelledDomainEventHandler` implementing `INotificationHandler<OrderCancelledDomainEvent>`
- [ ] Each handler transforms domain event to corresponding integration event from `EShop.Contracts`
- [ ] Publish integration event via `IPublishEndpoint` (MassTransit Bus Outbox stores in DB)
- [ ] Add appropriate logging in each handler
- [ ] Verify handlers are registered via MediatR assembly scanning

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 4.2: Order Events)

---
## Notes
(Updated during implementation)
