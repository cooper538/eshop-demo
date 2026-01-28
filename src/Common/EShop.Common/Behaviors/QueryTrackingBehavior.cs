using EShop.Common.Cqrs;
using EShop.Common.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Common.Behaviors;

/// <summary>
/// Pipeline behavior that automatically sets NoTracking for all queries.
/// </summary>
public sealed class QueryTrackingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IQuery<TResponse>
{
    private readonly IChangeTrackerAccessor? _changeTrackerAccessor;

    public QueryTrackingBehavior(IChangeTrackerAccessor? changeTrackerAccessor = null)
    {
        _changeTrackerAccessor = changeTrackerAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (_changeTrackerAccessor is not null)
        {
            _changeTrackerAccessor.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;
        }

        return await next();
    }
}
