namespace EShop.Contracts.Events.Product;

public sealed record StockLowEvent(
    Guid ProductId,
    string ProductName,
    int CurrentQuantity,
    int Threshold
) : IntegrationEvent;
