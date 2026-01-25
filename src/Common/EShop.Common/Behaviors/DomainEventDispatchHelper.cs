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
    public static async Task DispatchDomainEventsAsync(
        IChangeTrackerAccessor? changeTrackerAccessor,
        IDomainEventDispatcher eventDispatcher,
        CancellationToken cancellationToken
    )
    {
        if (changeTrackerAccessor is null)
        {
            return;
        }

        var domainEvents = CollectDomainEvents(changeTrackerAccessor);

        if (domainEvents.Count > 0)
        {
            await eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }
    }

    private static List<IDomainEvent> CollectDomainEvents(
        IChangeTrackerAccessor changeTrackerAccessor
    )
    {
        var entities = changeTrackerAccessor
            .ChangeTracker.Entries<Entity>()
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
