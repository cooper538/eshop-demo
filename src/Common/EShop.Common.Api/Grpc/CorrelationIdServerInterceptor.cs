using EShop.Common.Application.Correlation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace EShop.Common.Api.Grpc;

public sealed class CorrelationIdServerInterceptor : Interceptor
{
    private readonly ILogger<CorrelationIdServerInterceptor> _logger;

    public CorrelationIdServerInterceptor(ILogger<CorrelationIdServerInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation
    )
    {
        var correlationId =
            context
                .RequestHeaders.FirstOrDefault(h => h.Key == CorrelationIdConstants.GrpcMetadataKey)
                ?.Value
            ?? Guid.NewGuid().ToString();

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
            return await continuation(request, context);
        }
    }
}
