namespace EShop.Contracts.Dtos.Product;

public sealed record ProductDto(Guid Id, string Name, decimal Price, int StockQuantity);
