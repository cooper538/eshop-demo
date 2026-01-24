using EShop.Common.Correlation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EShop.Common.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers[CorrelationIdConstants.HttpHeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdConstants.HttpHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (CorrelationContext.CreateScope(correlationId))
        using (
            _logger.BeginScope(
                new Dictionary<string, object>
                {
                    [CorrelationIdConstants.LoggingScopeKey] = correlationId,
                }
            )
        )
        {
            await _next(context);
        }
    }
}
