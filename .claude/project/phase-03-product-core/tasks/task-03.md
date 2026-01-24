# Task 03: DbContext & EF Core

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
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

### Known Limitation: IProductDbContext with DbSet<T>

**Decision**: Using `DbSet<T>` directly in `IProductDbContext` interface.

**Why this is controversial** ([jasontaylordev/CleanArchitecture#482](https://github.com/jasontaylordev/CleanArchitecture/discussions/482)):
- Application layer has direct dependency on `Microsoft.EntityFrameworkCore` (via `DbSet<T>`)
- Violates Clean Architecture's **Dependency Rule** - infrastructure shouldn't leak into business logic
- EF Core team refused to provide `IDbSet<T>` ([dotnet/efcore#16470](https://github.com/dotnet/efcore/issues/16470))

**Jason Taylor's pragmatic defense**:
- EF Core's DbContext already acts as Unit of Work + Repository
- Additional abstraction layers don't always justify their complexity
- Use SQLite in-memory for unit tests, real DB for integration tests (avoid mocking DbContext)
- Template is a "starting point", not one-size-fits-all solution

**Trade-offs accepted**:
- ✅ Less boilerplate (no repository layer)
- ✅ Full LINQ/EF Core power in Application layer
- ✅ Testable with InMemory/SQLite providers
- ❌ Coupled to EF Core (switching ORM = rewrite)
- ❌ Not "pure" Clean Architecture
