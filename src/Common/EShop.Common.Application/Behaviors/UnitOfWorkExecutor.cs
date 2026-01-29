using EShop.Common.Application.Data;
using EShop.Common.Application.Events;
using EShop.Common.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Common.Application.Behaviors;

/// <summary>
/// Shared executor for UnitOfWork pipeline behaviors.
/// Dispatches domain events and saves changes atomically.
/// </summary>
public sealed class UnitOfWorkExecutor
{
    private const int MaxDispatchLoops = 10;

    private readonly IUnitOfWork? _unitOfWork;
    private readonly IChangeTrackerAccessor? _changeTrackerAccessor;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<UnitOfWorkExecutor> _logger;

    public UnitOfWorkExecutor(
        IDomainEventDispatcher eventDispatcher,
        ILogger<UnitOfWorkExecutor> logger,
        IUnitOfWork? unitOfWork = null,
        IChangeTrackerAccessor? changeTrackerAccessor = null
    )
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _changeTrackerAccessor = changeTrackerAccessor;
    }

    public async Task<TResponse> ExecuteAsync<TResponse>(
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var response = await next();

        await DispatchDomainEventsWithLoopAsync(cancellationToken);

        await SaveChangesAsync(cancellationToken);

        return response;
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
        {
            _logger.LogWarning(
                "UnitOfWork is null - changes will NOT be saved. "
                    + "Ensure IUnitOfWork is registered for this scope"
            );
            return;
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("Entity was modified by another user.", ex);
        }
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
