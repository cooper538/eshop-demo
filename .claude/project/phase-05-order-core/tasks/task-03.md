# Task 03: DbContext & EF Core

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | :white_circle: pending |
| Dependencies | task-01, task-02 |

## Summary
Configure EF Core DbContext with PostgreSQL, including OrderItem as owned entity.

## Scope

### Application Layer
- [ ] Create `IOrderDbContext` interface in Application/Data
  ```csharp
  public interface IOrderDbContext
  {
      DbSet<OrderEntity> Orders { get; }
      Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
  }
  ```

### Infrastructure Layer
- [ ] Create `OrderDbContext` in Infrastructure/Data
  - Implement `IOrderDbContext`
  - Implement `IChangeTrackerAccessor` (for MediatR behaviors)
  - Apply configurations from assembly

- [ ] Create `OrderConfiguration` in Infrastructure/Data/Configurations
  - Inherit from `AggregateRootConfiguration<OrderEntity>`
  - Configure OrderItem as **owned entity**:
    ```csharp
    builder.OwnsMany(o => o.Items, itemBuilder =>
    {
        itemBuilder.Property(i => i.ProductId).IsRequired();
        itemBuilder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        itemBuilder.Property(i => i.Quantity).IsRequired();
        itemBuilder.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
    });
    ```
  - Configure properties:
    - CustomerId: required
    - CustomerEmail: HasMaxLength(320), required
    - Status: required, stored as string (or int)
    - TotalAmount: HasPrecision(18, 2)
    - RejectionReason: HasMaxLength(500)

### Dependency Injection
- [ ] Create `DependencyInjection.cs` with `AddInfrastructure()` extension
  - Register DbContext with Aspire: `builder.AddNpgsqlDbContext<OrderDbContext>("orderdb")`
  - Register `IOrderDbContext` → `OrderDbContext`
  - Register `IChangeTrackerAccessor` → `OrderDbContext`

## Key Decision: OrderItem as Owned Entity
OrderItem is configured as **owned entity** (not standalone), meaning:
- No separate `OrderItems` table - stored within Orders table or as JSON
- No separate `DbSet<OrderItem>` in context
- Loaded automatically with Order (no explicit Include needed)
- Cannot exist without parent Order

## Reference Implementation
See `ProductDbContext` and `ProductConfiguration` in Products.Infrastructure

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 9.1: DbContext Interface Pattern)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (PostgreSQL configuration)

---
## Notes
(Updated during implementation)

### Known Limitation: IOrderDbContext with DbSet<T>
Same trade-off as Product Service - see task-03 notes from phase-03.
