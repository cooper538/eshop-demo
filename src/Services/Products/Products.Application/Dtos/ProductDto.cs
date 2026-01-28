using Products.Domain.Entities;

namespace Products.Application.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category
)
{
    public static ProductDto FromEntity(ProductEntity product, StockEntity stock)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            stock.Quantity,
            product.Category
        );
    }

    public static ProductDto FromEntity(ProductEntity product, int stockQuantity)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            stockQuantity,
            product.Category
        );
    }
}
