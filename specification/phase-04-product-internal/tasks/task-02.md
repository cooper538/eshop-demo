# Task 02: StockReservation Domain Entity

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create StockReservationEntity and EReservationStatus enum to track stock reservations linked to orders with TTL support.

## Scope
- [x] Create EReservationStatus enum (Active, Released, Expired)
- [x] Create StockReservationEntity inheriting from Entity (SharedKernel)
- [x] Add properties: StockId, OrderId, ProductId, Quantity, ReservedAt, ExpiresAt, Status
- [x] Implement internal static Create() factory method with configurable reservation duration
- [x] Implement Release() method for state transition (Active -> Released)
- [x] Implement Expire() method for state transition (Active -> Expired)
- [x] Add validation in Release/Expire to prevent invalid state transitions

## Implementation Details

**Files**:
- `Products.Domain/Enums/EReservationStatus.cs`
- `Products.Domain/Entities/StockReservationEntity.cs`

**Entity Properties**:
| Property | Type | Description |
|----------|------|-------------|
| StockId | Guid | FK to parent Stock aggregate |
| OrderId | Guid | Order that reserved the stock |
| ProductId | Guid | Product being reserved |
| Quantity | int | Reserved quantity |
| ReservedAt | DateTime | When reservation was created |
| ExpiresAt | DateTime | When reservation expires (ReservedAt + duration) |
| Status | EReservationStatus | Current reservation state |

**State Machine**:
```
Active -> Released (via Release())
Active -> Expired (via Expire())
```

**Design Decision**:
- StockReservation is child entity of Stock aggregate (created via Stock.ReserveStock)
- Create() is internal - only Stock aggregate can create reservations
- No ReleasedAt/ExpiredAt timestamps - status enum is sufficient

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 5.2: Stock Reservation)

---
## Notes
(Updated during implementation)
