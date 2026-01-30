using EShop.SharedKernel.Events;

namespace EShop.SharedKernel.Domain;

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Concurrency version for optimistic locking.
    /// Incremented whenever aggregate state changes.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Increments the version to mark aggregate as modified.
    /// Call this in any method that changes aggregate state (including child entity changes).
    /// </summary>
    protected void IncrementVersion() => Version++;
}
