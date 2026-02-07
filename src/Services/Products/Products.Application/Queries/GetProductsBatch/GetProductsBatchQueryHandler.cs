using EShop.Products.Application.Data;
using EShop.Products.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Products.Application.Queries.GetProductsBatch;

public sealed class GetProductsBatchQueryHandler
    : IRequestHandler<GetProductsBatchQuery, GetProductsBatchResult>
{
    private readonly IProductDbContext _dbContext;

    public GetProductsBatchQueryHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProductsBatchResult> Handle(
        GetProductsBatchQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _dbContext.Products.AsQueryable();

        if (request.ProductIds is not null)
        {
            query = query.Where(p => request.ProductIds.Contains(p.Id));
        }

        var products = await query
            .Join(
                _dbContext.Stocks,
                p => p.Id,
                s => s.ProductId,
                (p, s) => new ProductInfoDto(p.Id, p.Name, p.Description, p.Price, s.Quantity)
            )
            .ToListAsync(cancellationToken);

        return new GetProductsBatchResult(products);
    }
}
