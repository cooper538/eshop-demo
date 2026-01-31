using EShop.Common.Application.Cqrs;
using EShop.Products.Application.Dtos;

namespace EShop.Products.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
