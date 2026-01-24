# Task 03: DbContext & EF Core

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ⚪ pending |
| Dependencies | task-01, task-02 |

## Summary
Configure EF Core DbContext with PostgreSQL and entity configurations.

## Scope
- [ ] Create IProductDbContext interface in Application/Data
- [ ] Create ProductDbContext in Infrastructure/Data implementing IProductDbContext
- [ ] Create ProductConfiguration (entity configuration) in Infrastructure/Data/Configurations
- [ ] Configure optimistic concurrency with Version property
- [ ] Register DbContext in DI with PostgreSQL connection
- [ ] Add Aspire PostgreSQL component integration

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 9.1: DbContext Interface Pattern)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (PostgreSQL configuration)

---
## Notes
(Updated during implementation)
