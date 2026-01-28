using EShop.Common.Cqrs;
using EShop.Common.Data;
using EShop.Common.Events;
using EShop.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Common.Behaviors;

/// <summary>
/// Pipeline behavior that dispatches domain events BEFORE SaveChanges.
/// This ensures all changes (from command handler and domain event handlers) are committed atomically.
/// Must be registered LAST in the pipeline.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private const int MaxDispatchLoops = 10;

    private readonly IUnitOfWork? _unitOfWork;
    private readonly IChangeTrackerAccessor? _changeTrackerAccessor;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public UnitOfWorkBehavior(
        IDomainEventDispatcher eventDispatcher,
        IUnitOfWork? unitOfWork = null,
        IChangeTrackerAccessor? changeTrackerAccessor = null
    )
    {
        _eventDispatcher = eventDispatcher;
        _unitOfWork = unitOfWork;
        _changeTrackerAccessor = changeTrackerAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        // Execute handler (WITHOUT SaveChanges)
        var response = await next();

        // Dispatch domain events in a loop (handlers may raise new events)
        await DispatchDomainEventsWithLoopAsync(cancellationToken);

        // Single SaveChanges for all modifications
        if (_unitOfWork is not null)
        {
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConflictException("Entity was modified by another user.", ex);
            }
        }

        return response;
    }

    private async Task DispatchDomainEventsWithLoopAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < MaxDispatchLoops; i++)
        {
            var hasEvents = await DomainEventDispatchHelper.DispatchDomainEventsAsync(
                _changeTrackerAccessor,
                _eventDispatcher,
                cancellationToken
            );

            if (!hasEvents)
            {
                return;
            }
        }

        throw new InvalidOperationException(
            $"Domain event dispatch loop exceeded {MaxDispatchLoops} iterations. "
                + "This may indicate circular event dependencies."
        );
    }
}
