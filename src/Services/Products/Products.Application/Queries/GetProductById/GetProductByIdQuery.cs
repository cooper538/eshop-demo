using MediatR;
using Products.Application.Dtos;

namespace Products.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
