# Task 03: StockReservation Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-02 |

## Summary
Add database support for StockReservation entity - EF configuration and migration.

## Scope
- [x] Create StockReservationConfiguration with proper column mappings
- [x] Configure StockId as required FK
- [x] Add index on StockId
- [x] Add index on OrderId
- [x] Add index on ProductId
- [x] Add composite index on (Status, ExpiresAt) for expiration queries
- [x] Create and apply EF migration

## Implementation Details

**File**: `Products.Infrastructure/Data/Configurations/StockReservationConfiguration.cs`

**Indexes**:
| Index | Purpose |
|-------|---------|
| StockId | Parent aggregate lookup |
| OrderId | Find reservations by order |
| ProductId | Find reservations by product |
| (Status, ExpiresAt) | Efficient expiration queries |

**Design Decision**:
- StockReservation is owned by Stock aggregate (no standalone DbSet)
- Access via `Stock.Reservations` navigation property
- Child entities protected by aggregate root's concurrency token

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 10: Database Schema)

---
## Notes
(Updated during implementation)
