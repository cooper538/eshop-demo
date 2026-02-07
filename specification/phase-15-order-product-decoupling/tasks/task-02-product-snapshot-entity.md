# Task 02: ProductSnapshot Entity and Migration

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ done |
| Dependencies | - |

## Summary
Create the `ProductSnapshot` read model entity in Order service with EF Core configuration and database migration.

## Scope
- [ ] Create `ProductSnapshot` class in `Order.Domain/ReadModels/` (plain class, not `Entity`/`AggregateRoot`)
- [ ] Create `ProductSnapshotConfiguration` using `IEntityTypeConfiguration` (not `AggregateRootConfiguration`)
- [ ] Register `DbSet<ProductSnapshot>` in `IOrderDbContext` and `OrderDbContext`
- [ ] Generate EF Core migration

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md)
- → PLAN.md (Phase 2 -- naming rationale and convention alignment)

---
## Notes
(Updated during implementation)
