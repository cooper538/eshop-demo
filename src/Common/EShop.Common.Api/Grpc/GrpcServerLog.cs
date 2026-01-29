using Microsoft.Extensions.Logging;

namespace EShop.Common.Api.Grpc;

public static partial class GrpcServerLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "gRPC {Method} called")]
    public static partial void GrpcMethodCalled(this ILogger logger, string method);

    [LoggerMessage(Level = LogLevel.Debug, Message = "gRPC {Method} called for {Count} items")]
    public static partial void GrpcMethodCalledWithCount(
        this ILogger logger,
        string method,
        int count
    );

    [LoggerMessage(Level = LogLevel.Debug, Message = "gRPC {Method} called for OrderId: {OrderId}")]
    public static partial void GrpcMethodCalledForOrder(
        this ILogger logger,
        string method,
        string orderId
    );

    [LoggerMessage(Level = LogLevel.Warning, Message = "gRPC {Method} validation failed: {Errors}")]
    public static partial void GrpcValidationFailed(
        this ILogger logger,
        string method,
        string errors
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "gRPC {Method} resource not found: {Details}"
    )]
    public static partial void GrpcNotFound(this ILogger logger, string method, string details);

    [LoggerMessage(Level = LogLevel.Error, Message = "gRPC {Method} failed")]
    public static partial void GrpcMethodFailed(this ILogger logger, Exception ex, string method);

    [LoggerMessage(Level = LogLevel.Debug, Message = "gRPC call started: {Method}")]
    public static partial void GrpcCallStarted(this ILogger logger, string method);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "gRPC call completed: {Method} in {ElapsedMs}ms"
    )]
    public static partial void GrpcCallCompleted(
        this ILogger logger,
        string method,
        long elapsedMs
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "gRPC call failed: {Method} after {ElapsedMs}ms"
    )]
    public static partial void GrpcCallFailed(
        this ILogger logger,
        string method,
        long elapsedMs,
        Exception ex
    );
}
