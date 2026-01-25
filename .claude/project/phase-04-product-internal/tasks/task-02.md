# Task 02: StockReservation Domain Entity

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create StockReservation entity and EReservationStatusType enum to track stock reservations linked to orders with TTL support.

## Scope
- [x] Create EReservationStatusType enum (Active, Released, Expired)
- [x] Create StockReservation entity inheriting from Entity (SharedKernel)
- [x] Add properties: OrderId, ProductId, Quantity, ReservedAt, ExpiresAt, ReleasedAt, Status
- [x] Implement static Create() factory method with 15-minute TTL
- [x] Implement Release() method for state transition
- [x] Implement Expire() method for state transition

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 5.2: Stock Reservation)

---
## Notes
(Updated during implementation)
