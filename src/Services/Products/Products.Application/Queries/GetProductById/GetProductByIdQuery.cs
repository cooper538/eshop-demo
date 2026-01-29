using EShop.Common.Application.Cqrs;
using Products.Application.Dtos;

namespace Products.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
