namespace EShop.SharedKernel.Events;

public abstract record DomainEventBase : IDomainEvent
{
    public required DateTime OccurredOn { get; init; }
}
