using Microsoft.Extensions.Logging;

namespace EShop.ServiceClients.Infrastructure;

public static partial class ServiceClientLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "gRPC call started: {Operation}")]
    public static partial void GrpcCallStarted(this ILogger logger, string operation);

    [LoggerMessage(Level = LogLevel.Error, Message = "gRPC error in {Operation}")]
    public static partial void GrpcError(this ILogger logger, Exception ex, string operation);
}
