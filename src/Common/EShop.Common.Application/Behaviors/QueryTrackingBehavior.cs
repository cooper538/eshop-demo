using EShop.Common.Application.Cqrs;
using EShop.Common.Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Common.Application.Behaviors;

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
