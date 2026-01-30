# Task 03: StockReservation Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-02 |

## Summary
Add database support for StockReservation entity - DbSet, EF configuration, and migration.

## Scope
- [x] Extend IProductDbContext with DbSet<StockReservation>
- [x] Update ProductDbContext with StockReservations DbSet
- [x] Create StockReservationConfiguration with proper column mappings
- [x] Add index on OrderId
- [x] Add index on ProductId
- [x] Add composite index on (Status, ExpiresAt) for expiration queries
- [x] Create and apply EF migration

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 10: Database Schema - StockReservations Table)

---
## Notes
(Updated during implementation)
