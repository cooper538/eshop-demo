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

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Row version for optimistic concurrency control.
    /// Value is automatically generated and managed by the database.
    /// </summary>
    public byte[] Version { get; protected set; } = null!;
}
