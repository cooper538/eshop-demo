using Products.Domain.Entities;

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
)
{
    public static ProductDto FromEntity(ProductEntity entity)
    {
        return new ProductDto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Price,
            entity.StockQuantity,
            entity.Category
        );
    }
}
