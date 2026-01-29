namespace EShop.Contracts.IntegrationEvents.Product;

public sealed record ProductCreatedEvent(Guid ProductId, string Name, decimal Price)
    : IntegrationEvent;
