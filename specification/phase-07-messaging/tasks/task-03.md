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
- [x] Change `OrderEntity` to extend `AggregateRoot` instead of `Entity`
- [x] Add `AddDomainEvent(new OrderConfirmedDomainEvent(...))` in `OrderEntity.Confirm()`
- [x] Add `AddDomainEvent(new OrderRejectedDomainEvent(...))` in `OrderEntity.Reject()`
- [x] Add `AddDomainEvent(new OrderCancelledDomainEvent(...))` in `OrderEntity.Cancel()`
- [x] Call `IncrementVersion()` in each state transition method
- [x] Ensure all required data is passed to domain events
- [x] Verify solution builds successfully

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5.4: Publishing Flow)

---
## Notes
- Location: `src/Services/Order/Order.Domain/Entities/OrderEntity.cs`
- `Confirm()` creates `OrderConfirmedDomainEvent` with items mapped to `OrderItemInfo`
- `Reject(reason)` creates `OrderRejectedDomainEvent` with reason
- `Cancel(reason)` creates `OrderCancelledDomainEvent` with reason
- All methods set `OccurredOn` from the passed timestamp parameter
