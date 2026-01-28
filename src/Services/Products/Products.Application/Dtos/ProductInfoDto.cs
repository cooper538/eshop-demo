namespace Products.Application.Dtos;

/// <summary>
/// Product info DTO for internal API (gRPC).
/// </summary>
public sealed record ProductInfoDto(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);
