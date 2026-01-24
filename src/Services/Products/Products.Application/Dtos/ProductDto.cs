namespace Products.Application.Dtos;

/// <summary>
/// DTO for product API responses.
/// </summary>
public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category
);
