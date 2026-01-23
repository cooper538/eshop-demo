using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace EShop.ServiceClients.Infrastructure.Grpc;

internal sealed class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation
    )
    {
        var operation = context.Method.Name;

        _logger.GrpcCallStarted(operation);

        var call = continuation(request, context);

        return new AsyncUnaryCall<TResponse>(
            HandleResponse(call.ResponseAsync, operation),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose
        );
    }

    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> responseTask, string operation)
    {
        try
        {
            return await responseTask;
        }
        catch (RpcException ex)
        {
            _logger.GrpcError(ex, operation);
            throw ex.ToServiceClientException(operation);
        }
    }
}
