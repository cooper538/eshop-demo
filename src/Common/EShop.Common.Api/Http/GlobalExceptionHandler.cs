using EShop.Common.Application.Correlation;
using EShop.Common.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EShop.Common.Api.Http;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ICorrelationIdAccessor correlationIdAccessor,
        ILogger<GlobalExceptionHandler> logger
    )
    {
        _correlationIdAccessor = correlationIdAccessor;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ValidationException => (StatusCodes.Status400BadRequest, "Bad Request"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. CorrelationId: {CorrelationId}",
                _correlationIdAccessor.CorrelationId
            );
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        if (statusCode >= 500)
        {
            problemDetails.Extensions["correlationId"] = _correlationIdAccessor.CorrelationId;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
