using EShop.Common.Application.Cqrs;

namespace Products.Application.Queries.GetProducts;

public sealed record GetProductsQuery(string? Category, int Page, int PageSize)
    : IQuery<GetProductsResult>;
