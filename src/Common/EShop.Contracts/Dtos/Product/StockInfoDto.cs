namespace EShop.Contracts.Dtos.Product;

public sealed record StockInfoDto(Guid ProductId, int AvailableQuantity, int ReservedQuantity);
