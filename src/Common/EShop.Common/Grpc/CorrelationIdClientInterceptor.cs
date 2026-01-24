using EShop.Common.Correlation;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace EShop.Common.Grpc;

public sealed class CorrelationIdClientInterceptor : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation
    )
    {
        var correlationId = CorrelationContext.Current?.CorrelationId ?? Guid.NewGuid().ToString();

        var metadata = context.Options.Headers ?? new Metadata();
        metadata.Add(CorrelationIdConstants.GrpcMetadataKey, correlationId);

        var newOptions = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            newOptions
        );

        return continuation(request, newContext);
    }
}
