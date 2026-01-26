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
