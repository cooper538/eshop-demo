using Products.Domain.Entities;

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
)
{
    public static ProductInfoDto FromEntity(ProductEntity entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.Price, entity.StockQuantity);
}
