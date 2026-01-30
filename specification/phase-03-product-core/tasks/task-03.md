# Task 03: DbContext & EF Core

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01, task-02 |

## Summary
Configure EF Core DbContext with PostgreSQL, entity configurations, and MassTransit outbox integration.

## Scope
- [x] Create IProductDbContext interface in Application/Data
- [x] Create ProductDbContext in Infrastructure/Data implementing IProductDbContext, IChangeTrackerAccessor, IUnitOfWork
- [x] Create ProductConfiguration in Infrastructure/Data/Configurations
- [x] Create StockConfiguration in Infrastructure/Data/Configurations
- [x] Create StockReservationConfiguration in Infrastructure/Data/Configurations
- [x] Configure optimistic concurrency with Version property (RowVersion)
- [x] Register DbContext in DI with PostgreSQL connection via Aspire
- [x] Add MassTransit Outbox entities (InboxState, OutboxMessage, OutboxState)
- [x] Use RemoveEntitySuffixConvention for cleaner table names

## Related Specs
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section 9.1: DbContext Interface Pattern)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (PostgreSQL configuration)

---
## Notes
**Implemented interfaces**:
- `IProductDbContext` - DbSet accessors for Products, Stocks
- `IChangeTrackerAccessor` - for domain event dispatching
- `IUnitOfWork` - SaveChangesAsync, BeginTransactionAsync

**Entity configurations**:
- ProductConfiguration - table "Product", index on Category
- StockConfiguration - table "Stock", unique index on ProductId
- StockReservationConfiguration - table "StockReservation", indexes on OrderId, StockId, Status

**MassTransit Outbox**:
```csharp
modelBuilder.AddInboxStateEntity();
modelBuilder.AddOutboxMessageEntity();
modelBuilder.AddOutboxStateEntity();
```

### Known Limitation: IProductDbContext with DbSet<T>

**Decision**: Using `DbSet<T>` directly in `IProductDbContext` interface.

**Why this is controversial** ([jasontaylordev/CleanArchitecture#482](https://github.com/jasontaylordev/CleanArchitecture/discussions/482)):
- Application layer has direct dependency on `Microsoft.EntityFrameworkCore` (via `DbSet<T>`)
- Violates Clean Architecture's **Dependency Rule** - infrastructure shouldn't leak into business logic

**Trade-offs accepted**:
- ✅ Less boilerplate (no repository layer)
- ✅ Full LINQ/EF Core power in Application layer
- ❌ Coupled to EF Core (switching ORM = rewrite)
