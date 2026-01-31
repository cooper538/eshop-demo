using EShop.Products.Application.Dtos;

namespace EShop.Products.Application.Queries.GetProductsBatch;

public sealed record GetProductsBatchResult(IReadOnlyList<ProductInfoDto> Products);
