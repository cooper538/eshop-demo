using EShop.Common.Application.Cqrs;
using EShop.Products.Application.Dtos;
using EShop.Products.Domain.Entities;

namespace EShop.Products.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    int LowStockThreshold,
    string Category
) : ICommand<ProductDto>;

public static class CreateProductCommandMapper
{
    public static ProductEntity ToEntity(this CreateProductCommand command, DateTime createdAt)
    {
        return ProductEntity.Create(
            command.Name,
            command.Description,
            command.Price,
            command.StockQuantity,
            command.LowStockThreshold,
            command.Category,
            createdAt
        );
    }
}
