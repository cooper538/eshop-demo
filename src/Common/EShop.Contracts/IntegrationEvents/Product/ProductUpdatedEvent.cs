namespace EShop.Contracts.IntegrationEvents.Product;

public sealed record ProductUpdatedEvent(Guid ProductId, string Name, decimal Price)
    : IntegrationEvent;
