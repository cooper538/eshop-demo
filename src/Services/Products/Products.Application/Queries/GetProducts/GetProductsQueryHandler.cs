using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Data;
using Products.Application.Dtos;

namespace Products.Application.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsResult>
{
    private readonly IProductDbContext _dbContext;

    public GetProductsQueryHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProductsResult> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _dbContext.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category == request.Category);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(
                _dbContext.Stocks,
                p => p.Id,
                s => s.ProductId,
                (p, s) =>
                    new ProductDto(p.Id, p.Name, p.Description, p.Price, s.Quantity, p.Category)
            )
            .ToListAsync(cancellationToken);

        return new GetProductsResult(items, request.Page, request.PageSize, totalCount, totalPages);
    }
}
