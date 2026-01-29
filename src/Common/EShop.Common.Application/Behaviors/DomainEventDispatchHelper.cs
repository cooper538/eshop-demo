using EShop.Common.Application.Data;
using EShop.Common.Application.Events;
using EShop.SharedKernel.Domain;
using EShop.SharedKernel.Events;

namespace EShop.Common.Application.Behaviors;

/// <summary>
/// Helper for collecting and dispatching domain events from tracked entities.
/// </summary>
internal static class DomainEventDispatchHelper
{
    /// <summary>
    /// Dispatches domain events from tracked entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// IMPORTANT: Events are cleared BEFORE dispatch to prevent infinite loops.
    /// If a handler fails mid-dispatch:
    /// </para>
    /// <list type="bullet">
    ///   <item>Side effects (HTTP calls, external services) may have already occurred</item>
    ///   <item>SaveChanges will NOT be called (exception propagates)</item>
    ///   <item>On retry, events will be recreated by the command handler</item>
    /// </list>
    /// <para>
    /// Event handlers should be IDEMPOTENT for retry scenarios.
    /// Integration events use MassTransit Outbox for transactional delivery.
    /// </para>
    /// </remarks>
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
