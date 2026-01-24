using EShop.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Application.Dtos;
using Products.Domain.Entities;

namespace Products.Application.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductDbContext _dbContext;

    public GetProductByIdQueryHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDto> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(
            p => p.Id == request.Id,
            cancellationToken
        );

        if (product is null)
        {
            throw NotFoundException.For<ProductEntity>(request.Id);
        }

        return ProductDto.FromEntity(product);
    }
}
