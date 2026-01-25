using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Application.Dtos;

namespace Products.Application.Queries.GetProductsBatch;

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
        // Return only products that exist - caller validates completeness (ATOMIC check)
        var products = await _dbContext
            .Products.Where(p => request.ProductIds.Contains(p.Id))
            .Select(p => ProductInfoDto.FromEntity(p))
            .ToListAsync(cancellationToken);

        return new GetProductsBatchResult(products);
    }
}
