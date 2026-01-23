using EShop.ServiceClients.Configuration;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace EShop.ServiceClients.Infrastructure.Grpc;

internal sealed class ResilienceInterceptor : Interceptor
{
    private readonly AsyncRetryPolicy _retryPolicy;

    public ResilienceInterceptor(IOptions<ServiceClientOptions> options)
    {
        var retryOptions = options.Value.Resilience.Retry;

        _retryPolicy = Policy
            .Handle<RpcException>(ex => IsTransient(ex.StatusCode))
            .WaitAndRetryAsync(
                retryCount: retryOptions.MaxRetryCount,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * retryOptions.BaseDelayMs)
            );
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation
    )
    {
        var call = continuation(request, context);

        return new AsyncUnaryCall<TResponse>(
            ExecuteWithRetry(call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose
        );
    }

    private async Task<TResponse> ExecuteWithRetry<TResponse>(Task<TResponse> responseTask)
    {
        return await _retryPolicy.ExecuteAsync(async () => await responseTask);
    }

    private static bool IsTransient(StatusCode statusCode) =>
        statusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded or StatusCode.Aborted;
}
