using EShop.Common.Cqrs;
using Products.Application.Dtos;
using Products.Domain.Entities;

namespace Products.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    int LowStockThreshold,
    string Category
) : ICommand<ProductDto>;

public static class UpdateProductCommandMapper
{
    public static void ApplyTo(this UpdateProductCommand command, ProductEntity productEntity)
    {
        productEntity.Update(
            command.Name,
            command.Description,
            command.Price,
            command.StockQuantity,
            command.LowStockThreshold,
            command.Category
        );
    }
}
