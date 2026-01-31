using EShop.Common.Application.Cqrs;
using EShop.Products.Application.Dtos;
using EShop.Products.Domain.Entities;

namespace EShop.Products.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int LowStockThreshold,
    string Category
) : ICommand<ProductDto>;

public static class UpdateProductCommandMapper
{
    public static void ApplyToProduct(
        this UpdateProductCommand command,
        ProductEntity productEntity,
        DateTime updatedAt
    )
    {
        productEntity.Update(
            command.Name,
            command.Description,
            command.Price,
            command.Category,
            command.LowStockThreshold,
            updatedAt
        );
    }
}
