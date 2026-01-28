using EShop.Common.Cqrs;
using EShop.Common.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Common.Behaviors;

public sealed class CommandTrackingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IChangeTrackerAccessor? _changeTrackerAccessor;

    public CommandTrackingBehavior(IChangeTrackerAccessor? changeTrackerAccessor = null)
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
                QueryTrackingBehavior.TrackAll;
        }

        return await next();
    }
}
