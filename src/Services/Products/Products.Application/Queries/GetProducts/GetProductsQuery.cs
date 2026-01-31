using EShop.Common.Application.Cqrs;

namespace EShop.Products.Application.Queries.GetProducts;

public sealed record GetProductsQuery(string? Category, int Page, int PageSize)
    : IQuery<GetProductsResult>;
