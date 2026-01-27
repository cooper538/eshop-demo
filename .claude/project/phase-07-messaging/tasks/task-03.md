# Task 03: Order Entity Domain Events Integration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01, task-02 |

## Summary
Integrate domain event raising into Order entity state transition methods.

## Scope
- [ ] Change `Order` to extend `AggregateRoot` instead of `Entity`
- [ ] Add `AddDomainEvent(new OrderConfirmedDomainEvent(...))` in `Order.Confirm()`
- [ ] Add `AddDomainEvent(new OrderRejectedDomainEvent(...))` in `Order.Reject()`
- [ ] Add `AddDomainEvent(new OrderCancelledDomainEvent(...))` in `Order.Cancel()`
- [ ] Ensure all required data is passed to domain events
- [ ] Verify solution builds successfully

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5.4: Publishing Flow)

---
## Notes
(Updated during implementation)
