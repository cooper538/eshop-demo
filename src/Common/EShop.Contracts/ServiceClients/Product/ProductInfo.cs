namespace EShop.Contracts.ServiceClients.Product;

public sealed record ProductInfo(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);
