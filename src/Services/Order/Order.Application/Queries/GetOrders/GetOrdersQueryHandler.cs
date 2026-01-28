using MediatR;
using Microsoft.EntityFrameworkCore;
using Order.Application.Data;
using Order.Application.Dtos;

namespace Order.Application.Queries.GetOrders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResult>
{
    private readonly IOrderDbContext _dbContext;

    public GetOrdersQueryHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetOrdersResult> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _dbContext.Orders.Include(o => o.Items).AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => OrderDto.FromEntity(o))
            .ToListAsync(cancellationToken);

        return new GetOrdersResult(items, request.Page, request.PageSize, totalCount, totalPages);
    }
}
