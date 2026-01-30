# Task 02: Order Domain Events

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create domain events for Order aggregate state transitions (Confirmed, Rejected, Cancelled).

## Scope
- [ ] Create `OrderConfirmedDomainEvent` record with OrderId, CustomerId, CustomerEmail, TotalAmount, Items
- [ ] Create `OrderRejectedDomainEvent` record with OrderId, CustomerId, Reason
- [ ] Create `OrderCancelledDomainEvent` record with OrderId, CustomerId, Reason
- [ ] Create `OrderItemInfo` record for item details in confirmed event
- [ ] All events implement `IDomainEvent` with `OccurredAt` property
- [ ] Verify solution builds successfully

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 4.2: Order Events)

---
## Notes
(Updated during implementation)
