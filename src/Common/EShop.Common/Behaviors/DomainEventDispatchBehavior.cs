using EShop.Common.Cqrs;
using EShop.Common.Data;
using EShop.Common.Events;
using MediatR;

namespace EShop.Common.Behaviors;

/// <summary>
/// Pipeline behavior that dispatches domain events after command handler execution.
/// Must be registered LAST in the pipeline to run after SaveChangesAsync.
/// </summary>
public sealed class DomainEventDispatchBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IChangeTrackerAccessor? _changeTrackerAccessor;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public DomainEventDispatchBehavior(
        IDomainEventDispatcher eventDispatcher,
        IChangeTrackerAccessor? changeTrackerAccessor = null
    )
    {
        _eventDispatcher = eventDispatcher;
        _changeTrackerAccessor = changeTrackerAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        // Execute handler (includes SaveChangesAsync)
        var response = await next();

        // Collect and dispatch domain events after successful save
        await DomainEventDispatchHelper.DispatchDomainEventsAsync(
            _changeTrackerAccessor,
            _eventDispatcher,
            cancellationToken
        );

        return response;
    }
}
