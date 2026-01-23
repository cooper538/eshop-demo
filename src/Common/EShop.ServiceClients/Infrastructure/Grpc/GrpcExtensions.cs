using EShop.Common.Correlation;
using EShop.ServiceClients.Exceptions;
using Grpc.Core;

namespace EShop.ServiceClients.Infrastructure.Grpc;

public static class GrpcExtensions
{
    public static Metadata WithCorrelationId(this Metadata headers, string? correlationId)
    {
        if (!string.IsNullOrEmpty(correlationId))
        {
            headers.Add(CorrelationIdConstants.GrpcMetadataKey, correlationId);
        }

        return headers;
    }

    public static ServiceClientException ToServiceClientException(this RpcException ex, string operation) =>
        new($"Failed to {operation}: {ex.Status.Detail}", ex, ex.StatusCode.ToErrorCode());

    public static EServiceClientErrorCodeType ToErrorCode(this StatusCode statusCode) =>
        statusCode switch
        {
            StatusCode.NotFound => EServiceClientErrorCodeType.NotFound,
            StatusCode.InvalidArgument => EServiceClientErrorCodeType.ValidationError,
            StatusCode.Unavailable => EServiceClientErrorCodeType.ServiceUnavailable,
            StatusCode.DeadlineExceeded => EServiceClientErrorCodeType.Timeout,
            StatusCode.PermissionDenied => EServiceClientErrorCodeType.Unauthorized,
            _ => EServiceClientErrorCodeType.Unknown,
        };
}
