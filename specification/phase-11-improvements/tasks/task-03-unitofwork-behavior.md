# Task 03: UnitOfWork Behavior for Domain Events

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ⚪ pending |
| Dependencies | - |

## Summary
Refaktorovat dispatchování domain eventů tak, aby se spouštěly PŘED `SaveChangesAsync`, nikoliv po něm. Tím zajistíme, že všechny změny (z command handleru i domain event handlerů) budou commitnuty v jedné transakci.

## Motivation

**Problém s původním flow:**
```
Command Handler
    → Business logic (AddDomainEvent)
    → SaveChangesAsync() ✓
    → DomainEventDispatchBehavior
        → Domain event handlers běží (jejich změny se NEULOŽÍ!)
```

**Nový flow:**
```
Command Handler
    → Business logic (AddDomainEvent)
    → Returns (BEZ SaveChanges!)
    → UnitOfWorkBehavior
        → Dispatch domain events (loop pro kaskádové eventy)
        → Domain event handlers mohou modifikovat entity
        → SaveChangesAsync() - jediný commit pro vše
```

## Scope

### Core Infrastructure
- [ ] Create `IUnitOfWork` interface
- [ ] Create `UnitOfWorkBehavior<TRequest, TResponse>` for `ICommand<T>`
- [ ] Create `UnitOfWorkBehaviorUnit<TRequest, TResponse>` for `ICommand`
- [ ] Update `DomainEventDispatchHelper` to return bool (for loop detection)
- [ ] Add inner exception constructor to `ConflictException`

### DbContext Changes
- [ ] Remove `SaveChangesAsync` from `IProductDbContext`
- [ ] Remove `SaveChangesAsync` from `IOrderDbContext`
- [ ] Add `IUnitOfWork` interface to `ProductDbContext`
- [ ] Add `IUnitOfWork` interface to `OrderDbContext`

### Command Handlers (remove SaveChangesAsync)
- [ ] `CreateProductCommandHandler`
- [ ] `UpdateProductCommandHandler`
- [ ] `ReserveStockCommandHandler`
- [ ] `ReleaseStockCommandHandler`
- [ ] `ExpireReservationsCommandHandler`
- [ ] `CreateOrderCommandHandler`
- [ ] `CancelOrderCommandHandler`

### Event Handlers (remove SaveChangesAsync)
- [ ] `ProductUpdatedEventHandler`

### Cleanup & Registration
- [ ] Delete `DomainEventDispatchBehavior.cs`
- [ ] Delete `DomainEventDispatchBehaviorUnit.cs`
- [ ] Update `ServiceCollectionExtensions` - register UnitOfWork behaviors
- [ ] Register `IUnitOfWork` in Products.API/Program.cs
- [ ] Register `IUnitOfWork` in Order.Infrastructure/DependencyInjection.cs

### Background Job Fix
- [ ] Add `ConflictException` catch to `StockReservationExpirationJob`

## Key Implementation Details

### Loop Detection for Cascading Events
```csharp
private const int MaxDispatchLoops = 10;

for (var i = 0; i < MaxDispatchLoops; i++)
{
    var hasEvents = await DomainEventDispatchHelper.DispatchDomainEventsAsync(...);
    if (!hasEvents) return;
}
throw new InvalidOperationException("Circular event dependencies detected");
```

### Concurrency Handling
```csharp
try
{
    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
catch (DbUpdateConcurrencyException ex)
{
    throw new ConflictException("Entity was modified by another user.", ex);
}
```

### Pipeline Order (CRITICAL)
UnitOfWork behaviors MUST be registered LAST:
```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
// ... other behaviors ...
// UnitOfWork MUST be LAST - dispatches events then saves
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehaviorUnit<,>));
```

## Related Files

| Action | File |
|--------|------|
| CREATE | `EShop.Common/Data/IUnitOfWork.cs` |
| CREATE | `EShop.Common/Behaviors/UnitOfWorkBehavior.cs` |
| CREATE | `EShop.Common/Behaviors/UnitOfWorkBehaviorUnit.cs` |
| DELETE | `EShop.Common/Behaviors/DomainEventDispatchBehavior.cs` |
| DELETE | `EShop.Common/Behaviors/DomainEventDispatchBehaviorUnit.cs` |
| MODIFY | `EShop.Common/Behaviors/DomainEventDispatchHelper.cs` |
| MODIFY | `EShop.Common/Exceptions/ApplicationException.cs` |
| MODIFY | `EShop.Common/Exceptions/ConflictException.cs` |
| MODIFY | `EShop.Common/Extensions/ServiceCollectionExtensions.cs` |
| MODIFY | `Products.Application/Data/IProductDbContext.cs` |
| MODIFY | `Order.Application/Data/IOrderDbContext.cs` |
| MODIFY | `Products.Infrastructure/Data/ProductDbContext.cs` |
| MODIFY | `Order.Infrastructure/Data/OrderDbContext.cs` |
| MODIFY | All Command Handlers (7 files) |
| MODIFY | `ProductUpdatedEventHandler.cs` |
| MODIFY | `Products.API/Program.cs` |
| MODIFY | `Order.Infrastructure/DependencyInjection.cs` |
| MODIFY | `StockReservationExpirationJob.cs` |

## Verification

1. `dotnet build EShopDemo.sln` - must pass
2. `dotnet test EShopDemo.sln` - must pass
3. Manual test: Create product → verify Stock created via domain event in same transaction

## MassTransit Outbox Compatibility

MassTransit EF Outbox funguje správně i s novým přístupem:
1. Domain event handler volá `publishEndpoint.Publish(integrationEvent)`
2. MassTransit zapíše message do OutboxMessage tabulky (v paměti DbContextu)
3. UnitOfWorkBehavior volá SaveChangesAsync
4. Entities + OutboxMessages commitnuty atomicky

---
## Notes
(Updated during implementation)
