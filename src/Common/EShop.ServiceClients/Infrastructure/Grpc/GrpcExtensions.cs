using EShop.Contracts.ServiceClients;
using Grpc.Core;

namespace EShop.ServiceClients.Infrastructure.Grpc;

public static class GrpcExtensions
{
    public static ServiceClientException ToServiceClientException(
        this RpcException ex,
        string operation
    ) => new($"Failed to {operation}: {ex.Status.Detail}", ex, ex.StatusCode.ToErrorCode());

    public static EServiceClientErrorCode ToErrorCode(this StatusCode statusCode) =>
        statusCode switch
        {
            StatusCode.NotFound => EServiceClientErrorCode.NotFound,
            StatusCode.InvalidArgument => EServiceClientErrorCode.ValidationError,
            StatusCode.Unavailable => EServiceClientErrorCode.ServiceUnavailable,
            StatusCode.DeadlineExceeded => EServiceClientErrorCode.Timeout,
            StatusCode.PermissionDenied => EServiceClientErrorCode.Unauthorized,
            _ => EServiceClientErrorCode.Unknown,
        };
}
