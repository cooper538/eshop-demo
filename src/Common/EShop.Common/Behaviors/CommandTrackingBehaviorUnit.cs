using EShop.Common.Cqrs;
using EShop.Common.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Common.Behaviors;

/// <summary>
/// Pipeline behavior that automatically sets TrackAll for commands without result.
/// </summary>
public sealed class CommandTrackingBehaviorUnit<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
    where TResponse : notnull
{
    private readonly IChangeTrackerAccessor? _changeTrackerAccessor;

    public CommandTrackingBehaviorUnit(IChangeTrackerAccessor? changeTrackerAccessor = null)
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
