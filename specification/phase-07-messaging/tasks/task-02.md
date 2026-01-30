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
- [x] Create `OrderConfirmedDomainEvent` record with OrderId, CustomerId, CustomerEmail, TotalAmount, Items
- [x] Create `OrderRejectedDomainEvent` record with OrderId, CustomerId, CustomerEmail, Reason
- [x] Create `OrderCancelledDomainEvent` record with OrderId, CustomerId, CustomerEmail, Reason
- [x] Create `OrderItemInfo` record for item details in confirmed event
- [x] All events extend `DomainEventBase` (provides `OccurredOn` property)
- [x] Verify solution builds successfully

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 4.2: Order Events)

---
## Notes
- Location: `src/Services/Order/Order.Domain/Events/`
- All domain events extend `DomainEventBase` record
- CustomerEmail included in all events for notification purposes
- `OrderItemInfo(ProductId, ProductName, Quantity, UnitPrice)` used for order items
