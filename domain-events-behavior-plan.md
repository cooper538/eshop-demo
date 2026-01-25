# Plan: DomainEventDispatchBehavior Implementation

## Problem Statement

Current solution dispatches domain events in `DomainEventsDbContext.SaveChangesAsync()`:
- ❌ Couples persistence (DbContext) with application concern (event dispatch)
- ❌ DbContext requires `IDomainEventDispatcher` in constructor → breaks EF migrations
- ❌ Reflection in dispatcher not cached (performance)

## Proposed Solution

Move event dispatch to **MediatR behavior** that runs AFTER command handlers.

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Command Handler                                             │
│  ├── Business logic                                          │
│  ├── Entity.AddDomainEvent(event)                           │
│  └── DbContext.SaveChangesAsync() ← saves only, no dispatch │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  DomainEventDispatchBehavior (runs AFTER handler)           │
│  ├── Collect events from ChangeTracker.Entries<Entity>()   │
│  ├── Clear events from entities                             │
│  └── Dispatch via IDomainEventDispatcher                    │
└─────────────────────────────────────────────────────────────┘
```

### Key Design Decisions

1. **Behavior runs AFTER handler** - events dispatched only after successful command
2. **Only for ICommand** - queries don't dispatch events
3. **Background jobs** - use MediatR commands (consistent pattern)

## Implementation Steps

### Step 1: Create DomainEventDispatchBehavior

**File:** `src/Common/EShop.Common/Behaviors/DomainEventDispatchBehavior.cs`

```csharp
public sealed class DomainEventDispatchBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IChangeTrackerAccessor? _changeTrackerAccessor;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();  // Execute handler first

        if (_changeTrackerAccessor is null) return response;

        // Collect and dispatch events
        var entities = _changeTrackerAccessor.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var events = entities.SelectMany(e => e.DomainEvents).ToList();

        foreach (var entity in entities)
            entity.ClearDomainEvents();

        await _eventDispatcher.DispatchAsync(events, cancellationToken);

        return response;
    }
}
```

### Step 2: Create Unit variant for ICommand (no result)

**File:** Same file or `DomainEventDispatchBehaviorUnit.cs`

```csharp
public sealed class DomainEventDispatchBehaviorUnit<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
    where TResponse : Unit
{ ... }
```

### Step 3: Remove DomainEventsDbContext

**File:** `src/Common/EShop.Common/Data/DomainEventsDbContext.cs` → **DELETE**

ProductDbContext returns to inheriting directly from `DbContext`:
- No IDomainEventDispatcher dependency
- EF migrations work without issues
- Clean separation of concerns

### Step 4: Refactor Background Jobs to MediatR Commands

**Problem:** `StockReservationExpirationJob` doesn't use MediatR → no behavior

**Solution:** Job sends MediatR command instead of direct DbContext access

**New files:**
- `Products.Application/Commands/ExpireReservations/ExpireReservationsCommand.cs`
- `Products.Application/Commands/ExpireReservations/ExpireReservationsCommandHandler.cs`

**Job becomes:**
```csharp
public class StockReservationExpirationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new ExpireReservationsCommand(), ct);

            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }
    }
}
```

**Handler contains current expiration logic** - entities raise events, behavior dispatches them

### Step 5: Register Behavior

**File:** `src/Common/EShop.Common/Extensions/ServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddCommonBehaviors(this IServiceCollection services)
{
    // ... existing behaviors ...

    // Domain events - must be LAST to run after handler
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehaviorUnit<,>));

    return services;
}
```

### Step 6: Cache Reflection in Dispatcher

**File:** `src/Common/EShop.Common/Events/MediatRDomainEventDispatcher.cs`

```csharp
private static readonly ConcurrentDictionary<Type, ConstructorInfo> _constructorCache = new();

public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct)
{
    var eventType = domainEvent.GetType();
    var ctor = _constructorCache.GetOrAdd(eventType, type =>
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(type);
        return notificationType.GetConstructor([type])!;
    });

    var notification = ctor.Invoke([domainEvent]);
    await _publisher.Publish(notification, ct);
}
```

## Files to Modify

| File | Action |
|------|--------|
| `EShop.Common/Behaviors/DomainEventDispatchBehavior.cs` | CREATE |
| `EShop.Common/Behaviors/DomainEventDispatchBehaviorUnit.cs` | CREATE |
| `EShop.Common/Data/DomainEventsDbContext.cs` | DELETE |
| `EShop.Common/Events/MediatRDomainEventDispatcher.cs` | MODIFY - add cache |
| `EShop.Common/Extensions/ServiceCollectionExtensions.cs` | MODIFY - register behaviors |
| `Products.Infrastructure/Data/ProductDbContext.cs` | MODIFY - inherit DbContext directly |
| `Products.Application/Commands/ExpireReservations/ExpireReservationsCommand.cs` | CREATE |
| `Products.Application/Commands/ExpireReservations/ExpireReservationsCommandHandler.cs` | CREATE |
| `Products.Infrastructure/BackgroundJobs/StockReservationExpirationJob.cs` | MODIFY - use MediatR |

## Verification

1. **Build:** `dotnet build EShopDemo.sln`
2. **EF Migrations:** `dotnet ef migrations list -p src/Services/Products/Products.Infrastructure` (should work without factory)
3. **Manual test:** Run Aspire, trigger ReserveStock, verify events dispatched via logs
