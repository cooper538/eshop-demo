using Products.Application.Dtos;

namespace Products.Application.Queries.GetProductsBatch;

public sealed record GetProductsBatchResult(IReadOnlyList<ProductInfoDto> Products);
