using Products.Domain.Entities;

namespace Products.Application.Dtos;

public static class ProductMapper
{
    public static ProductDto ToDto(this ProductEntity productEntity)
    {
        return new ProductDto(
            productEntity.Id,
            productEntity.Name,
            productEntity.Description,
            productEntity.Price,
            productEntity.StockQuantity,
            productEntity.Category
        );
    }
}
