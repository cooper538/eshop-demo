namespace EShop.Contracts.IntegrationEvents.Product;

public sealed record StockLowEvent(
    Guid ProductId,
    string ProductName,
    int CurrentQuantity,
    int Threshold
) : IntegrationEvent;
