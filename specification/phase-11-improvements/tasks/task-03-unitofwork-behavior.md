# Task 03: UnitOfWork Behavior for Domain Events

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Refactored domain event dispatching to run BEFORE `SaveChangesAsync`. All changes (from command handler and domain event handlers) are committed atomically in one transaction.

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
- [x] Create `UnitOfWorkBehavior<TRequest, TResponse>` for `ICommand<T>`
- [x] Create `UnitOfWorkBehaviorUnit<TRequest, TResponse>` for `ICommand`
- [x] Create `UnitOfWorkExecutor` shared executor with loop detection
- [x] Update `DomainEventDispatchHelper` to support cascading events

### Pipeline Changes
- [x] Delete `DomainEventDispatchBehavior.cs`
- [x] Delete `DomainEventDispatchBehaviorUnit.cs`
- [x] Register UnitOfWork behaviors LAST in pipeline

### Command Handlers (SaveChangesAsync removed)
- [x] All command handlers no longer call SaveChangesAsync
- [x] UnitOfWorkExecutor handles atomic commit

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

## Implementation

### Key Files
- `src/Common/EShop.Common.Application/Behaviors/UnitOfWorkBehavior.cs`
- `src/Common/EShop.Common.Application/Behaviors/UnitOfWorkBehaviorUnit.cs`
- `src/Common/EShop.Common.Application/Behaviors/UnitOfWorkExecutor.cs`
- `src/Common/EShop.Common.Application/Behaviors/DomainEventDispatchHelper.cs`

### MassTransit Outbox Compatibility

MassTransit EF Outbox works correctly with this approach:
1. Domain event handler calls `publishEndpoint.Publish(integrationEvent)`
2. MassTransit writes message to OutboxMessage table (in DbContext memory)
3. UnitOfWorkExecutor calls SaveChangesAsync
4. Entities + OutboxMessages committed atomically

---
## Notes
Implemented with `UnitOfWorkExecutor` class that handles both ICommand<T> and ICommand behaviors to avoid code duplication.
