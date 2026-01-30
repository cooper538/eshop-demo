# Task 03: DbContext & EF Core

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | task-01, task-02 |

## Summary
Configure EF Core DbContext with PostgreSQL, including OrderItem as owned entity and MassTransit outbox support.

## Scope

### Application Layer - IMPLEMENTED
- [x] `IOrderDbContext` interface in Application/Data
  ```csharp
  public interface IOrderDbContext
  {
      DbSet<OrderEntity> Orders { get; }
  }
  ```

### Infrastructure Layer - IMPLEMENTED
- [x] `OrderDbContext` in Infrastructure/Data
  - Implements `IOrderDbContext`
  - Implements `IChangeTrackerAccessor` (for MediatR behaviors)
  - Implements `IUnitOfWork` (for transaction management)
  - Uses `RemoveEntitySuffixConvention` for table naming
  - **MassTransit Outbox entities**: InboxState, OutboxMessage, OutboxState

- [x] `OrderConfiguration` in Infrastructure/Data/Configurations
  - Inherits from `AggregateRootConfiguration<OrderEntity>`
  - Configures OrderItem as **owned entity** via `OwnsMany()`

### Dependency Injection - IMPLEMENTED
- [x] `DependencyInjection.cs` with `AddInfrastructure()` extension
  - Registers DbContext with Aspire: `builder.AddNpgsqlDbContext<OrderDbContext>("orderdb")`
  - Registers `IOrderDbContext` -> `OrderDbContext`
  - Registers `IChangeTrackerAccessor` -> `OrderDbContext`
  - Registers `IUnitOfWork` -> `OrderDbContext`
  - **Configures MassTransit messaging with outbox**
  - Registers FluentValidation validators

## Actual Implementation Structure
```
Order.Infrastructure/
├── Data/
│   ├── Configurations/
│   │   └── OrderConfiguration.cs
│   ├── Migrations/
│   │   ├── 20260129212019_InitialCreate.cs
│   │   ├── 20260129212019_InitialCreate.Designer.cs
│   │   └── OrderDbContextModelSnapshot.cs
│   └── OrderDbContext.cs
└── DependencyInjection.cs

Order.Application/
└── Data/
    └── IOrderDbContext.cs
```

## Key Decisions

### OrderItem as Owned Entity
OrderItem is configured as **owned entity**, meaning:
- Stored in separate `OrderItem` table (not inline JSON)
- No separate `DbSet<OrderItem>` in context
- Loaded automatically with Order
- Cannot exist without parent Order

### MassTransit Outbox (beyond original scope)
DbContext includes outbox pattern tables:
- `InboxState` - for exactly-once processing
- `OutboxMessage` - for reliable message publishing
- `OutboxState` - for outbox state management

## Reference Implementation
See `ProductDbContext` and `ProductConfiguration` in Products.Infrastructure

## Related Specs
- -> [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section 9.1: DbContext Interface Pattern)
- -> [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md) (PostgreSQL configuration)

---
## Notes
MassTransit outbox configured - originally planned for Phase 7 (Messaging).
