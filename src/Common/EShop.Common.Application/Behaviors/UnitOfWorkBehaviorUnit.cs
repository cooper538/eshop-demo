using EShop.Common.Application.Cqrs;
using MediatR;

namespace EShop.Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that dispatches domain events BEFORE SaveChanges (for commands without result).
/// This ensures all changes (from command handler and domain event handlers) are committed atomically.
/// Must be registered LAST in the pipeline.
/// </summary>
public sealed class UnitOfWorkBehaviorUnit<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
    where TResponse : notnull
{
    private readonly UnitOfWorkExecutor _executor;

    public UnitOfWorkBehaviorUnit(UnitOfWorkExecutor executor)
    {
        _executor = executor;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return _executor.ExecuteAsync(next, cancellationToken);
    }
}
