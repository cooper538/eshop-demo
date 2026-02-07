namespace EShop.Contracts.IntegrationEvents.Product;

public sealed record ProductChangedEvent(Guid ProductId, string Name, decimal Price)
    : IntegrationEvent;
