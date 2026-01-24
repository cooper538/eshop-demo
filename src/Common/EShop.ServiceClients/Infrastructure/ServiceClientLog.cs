using Microsoft.Extensions.Logging;

namespace EShop.ServiceClients.Infrastructure;

public static partial class ServiceClientLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "gRPC call started: {Operation}")]
    public static partial void GrpcCallStarted(this ILogger logger, string operation);

    [LoggerMessage(Level = LogLevel.Error, Message = "gRPC error in {Operation}")]
    public static partial void GrpcError(this ILogger logger, Exception ex, string operation);

    [LoggerMessage(Level = LogLevel.Debug, Message = "HTTP {Method} call started: {Operation}")]
    public static partial void HttpCallStarted(
        this ILogger logger,
        string method,
        string operation
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "HTTP {Method} call failed: {Operation} returned {StatusCode}"
    )]
    public static partial void HttpCallFailed(
        this ILogger logger,
        string method,
        string operation,
        int statusCode
    );

    [LoggerMessage(Level = LogLevel.Error, Message = "HTTP error in {Operation}")]
    public static partial void HttpError(this ILogger logger, Exception ex, string operation);
}
