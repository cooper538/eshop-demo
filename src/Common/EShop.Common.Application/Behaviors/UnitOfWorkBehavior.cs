using EShop.Common.Application.Cqrs;
using MediatR;

namespace EShop.Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that dispatches domain events BEFORE SaveChanges.
/// This ensures all changes (from command handler and domain event handlers) are committed atomically.
/// Must be registered LAST in the pipeline.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly UnitOfWorkExecutor _executor;

    public UnitOfWorkBehavior(UnitOfWorkExecutor executor)
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
