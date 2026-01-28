using EShop.Common.Data;
using EShop.Common.Events;
using EShop.SharedKernel.Domain;
using EShop.SharedKernel.Events;

namespace EShop.Common.Behaviors;

/// <summary>
/// Helper for collecting and dispatching domain events from tracked entities.
/// </summary>
internal static class DomainEventDispatchHelper
{
    /// <summary>
    /// Dispatches domain events from tracked entities.
    /// </summary>
    /// <returns>True if any events were dispatched, false otherwise.</returns>
    public static async Task<bool> DispatchDomainEventsAsync(
        IChangeTrackerAccessor? changeTrackerAccessor,
        IDomainEventDispatcher eventDispatcher,
        CancellationToken cancellationToken
    )
    {
        if (changeTrackerAccessor is null)
        {
            return false;
        }

        var domainEvents = CollectDomainEvents(changeTrackerAccessor);

        if (domainEvents.Count > 0)
        {
            await eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            return true;
        }

        return false;
    }

    private static List<IDomainEvent> CollectDomainEvents(
        IChangeTrackerAccessor changeTrackerAccessor
    )
    {
        var entities = changeTrackerAccessor
            .ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();

        // Clear events from entities to prevent duplicate dispatch
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        return domainEvents;
    }
}
