# Task 02: StockReservation Domain Entity

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | :white_circle: pending |
| Dependencies | - |

## Summary
Create StockReservation entity and EReservationStatusType enum to track stock reservations linked to orders with TTL support.

## Scope
- [ ] Create EReservationStatusType enum (Active, Released, Expired)
- [ ] Create StockReservation entity inheriting from Entity (SharedKernel)
- [ ] Add properties: OrderId, ProductId, Quantity, ReservedAt, ExpiresAt, ReleasedAt, Status
- [ ] Implement static Create() factory method with 15-minute TTL
- [ ] Implement Release() method for state transition
- [ ] Implement Expire() method for state transition

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 5.2: Stock Reservation)

---
## Notes
(Updated during implementation)
