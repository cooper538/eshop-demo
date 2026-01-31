using EShop.Common.Application.Exceptions;
using EShop.Products.Application.Data;
using EShop.Products.Application.Dtos;
using EShop.Products.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Products.Application.Queries.GetProductById;

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
        var result = await _dbContext
            .Products.Where(p => p.Id == request.Id)
            .Join(
                _dbContext.Stocks,
                p => p.Id,
                s => s.ProductId,
                (p, s) =>
                    new ProductDto(p.Id, p.Name, p.Description, p.Price, s.Quantity, p.Category)
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw NotFoundException.For<ProductEntity>(request.Id);
        }

        return result;
    }
}
