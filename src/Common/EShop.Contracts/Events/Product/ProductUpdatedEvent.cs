namespace EShop.Contracts.Events.Product;

public sealed record ProductUpdatedEvent(Guid ProductId, string Name, decimal Price)
    : IntegrationEvent;
