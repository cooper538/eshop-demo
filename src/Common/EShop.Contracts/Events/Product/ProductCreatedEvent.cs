namespace EShop.Contracts.Events.Product;

public sealed record ProductCreatedEvent(Guid ProductId, string Name, decimal Price) : IntegrationEvent;
