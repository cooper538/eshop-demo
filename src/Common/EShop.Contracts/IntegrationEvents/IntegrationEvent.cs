namespace EShop.Contracts.IntegrationEvents;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public required DateTime Timestamp { get; init; }
}
