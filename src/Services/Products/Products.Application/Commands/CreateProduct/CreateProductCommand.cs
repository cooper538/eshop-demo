using EShop.Common.Application.Cqrs;
using Products.Application.Dtos;
using Products.Domain.Entities;

namespace Products.Application.Commands.CreateProduct;

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
