using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace EShop.Common.Grpc;

public sealed class GrpcLoggingInterceptor : Interceptor
{
    private readonly ILogger<GrpcLoggingInterceptor> _logger;

    public GrpcLoggingInterceptor(ILogger<GrpcLoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation
    )
    {
        var method = context.Method;
        var stopwatch = Stopwatch.StartNew();

        _logger.GrpcCallStarted(method);

        try
        {
            var response = await continuation(request, context);
            stopwatch.Stop();
            _logger.GrpcCallCompleted(method, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.GrpcCallFailed(method, stopwatch.ElapsedMilliseconds, ex);
            throw;
        }
    }
}
