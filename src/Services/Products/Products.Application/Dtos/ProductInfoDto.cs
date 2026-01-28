namespace Products.Application.Dtos;

public sealed record ProductInfoDto(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);
