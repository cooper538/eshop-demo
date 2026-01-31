using EShop.Products.Application.Dtos;

namespace EShop.Products.Application.Queries.GetProducts;

public sealed record GetProductsResult(
    IReadOnlyList<ProductDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
