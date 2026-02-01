using EShop.Common.Application.Events;
using EShop.SharedKernel.Domain;
using EShop.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.Common.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that dispatches domain events BEFORE SaveChanges.
/// This ensures all changes (from command handler and domain event handlers) are committed atomically.
/// </summary>
public sealed class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private const int MaxDispatchLoops = 10;

    private readonly IDomainEventDispatcher _eventDispatcher;

    public DomainEventDispatchInterceptor(IDomainEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        if (eventData.Context is not null)
        {
            DispatchDomainEventsAsync(eventData.Context, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        return base.SavingChanges(eventData, result);
    }

    private async Task DispatchDomainEventsAsync(
        DbContext context,
        CancellationToken cancellationToken
    )
    {
        for (var i = 0; i < MaxDispatchLoops; i++)
        {
            var domainEvents = CollectDomainEvents(context);

            if (domainEvents.Count == 0)
            {
                return;
            }

            await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        throw new InvalidOperationException(
            $"Domain event dispatch loop exceeded {MaxDispatchLoops} iterations. "
                + "This may indicate circular event dependencies."
        );
    }

    private static List<IDomainEvent> CollectDomainEvents(DbContext context)
    {
        var entities = context
            .ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();

        // Clear events BEFORE dispatch to prevent duplicate dispatch
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        return domainEvents;
    }
}
