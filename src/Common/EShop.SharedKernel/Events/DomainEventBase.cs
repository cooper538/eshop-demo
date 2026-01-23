namespace EShop.SharedKernel.Events;

public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
