using MediatR;

namespace Products.Application.Queries.GetProducts;

public sealed record GetProductsQuery(string? Category, int Page, int PageSize)
    : IRequest<GetProductsResult>;
