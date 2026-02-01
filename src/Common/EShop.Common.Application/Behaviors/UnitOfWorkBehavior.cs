using EShop.Common.Application.Cqrs;
using EShop.Common.Application.Data;
using EShop.Common.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that saves changes after command handler completes.
/// Must be registered LAST in the pipeline.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IUnitOfWork? _unitOfWork;
    private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

    public UnitOfWorkBehavior(
        ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger,
        IUnitOfWork? unitOfWork = null
    )
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var response = await next();

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
}
