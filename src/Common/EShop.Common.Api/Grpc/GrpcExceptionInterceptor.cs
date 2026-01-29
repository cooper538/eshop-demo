using EShop.Common.Application.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace EShop.Common.Api.Grpc;

public sealed class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation
    )
    {
        try
        {
            return await continuation(request, context);
        }
        // Domain entity not found (e.g., product, order)
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        // Business validation from Application layer (e.g., insufficient stock)
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        // RpcException from other interceptors (e.g., GrpcValidationInterceptor) - pass through
        catch (RpcException)
        {
            throw;
        }
        // Unexpected errors - hide details from client for security
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in gRPC call: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."));
        }
    }
}
