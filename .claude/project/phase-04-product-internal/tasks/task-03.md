# Task 03: StockReservation Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | :white_circle: pending |
| Dependencies | task-02 |

## Summary
Add database support for StockReservation entity - DbSet, EF configuration, and migration.

## Scope
- [ ] Extend IProductDbContext with DbSet<StockReservation>
- [ ] Update ProductDbContext with StockReservations DbSet
- [ ] Create StockReservationConfiguration with proper column mappings
- [ ] Add index on OrderId
- [ ] Add index on ProductId
- [ ] Add composite index on (Status, ExpiresAt) for expiration queries
- [ ] Create and apply EF migration

## Related Specs
- [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 10: Database Schema - StockReservations Table)

---
## Notes
(Updated during implementation)
