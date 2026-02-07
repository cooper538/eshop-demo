using System.Globalization;
using EShop.Grpc.Product;
using EShop.Products.Application.Commands.ReleaseStock;
using EShop.Products.Application.Commands.ReserveStock;
using EShop.Products.Application.Dtos;
using EShop.Products.Application.Queries.GetProductsBatch;
using Grpc.Core;
using MediatR;

namespace EShop.Products.API.Grpc;

public sealed class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly IMediator _mediator;

    public ProductGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    private static GetProductsResponse MapToResponse(GetProductsBatchResult result)
    {
        var response = new GetProductsResponse();
        response.Products.AddRange(
            result.Products.Select(p => new ProductInfo
            {
                ProductId = p.ProductId.ToString(),
                Name = p.Name,
                Description = p.Description,
                Price = p.Price.ToString(CultureInfo.InvariantCulture),
                StockQuantity = p.StockQuantity,
            })
        );
        return response;
    }

    private static ErrorCode MapErrorCode(EStockReservationError errorCode)
    {
        return errorCode switch
        {
            EStockReservationError.None => ErrorCode.None,
            EStockReservationError.InsufficientStock => ErrorCode.InsufficientStock,
            EStockReservationError.ProductNotFound => ErrorCode.ProductNotFound,
            _ => ErrorCode.None,
        };
    }

    // Null validation suppressed: gRPC framework guarantees non-null request/context parameters.
    // Input validation (GUID format, empty values) is handled by GrpcValidationInterceptor.
#pragma warning disable CA1062
    public override async Task<GetProductsResponse> GetAllProducts(
        GetAllProductsRequest request,
        ServerCallContext context
    )
    {
        var query = new GetProductsBatchQuery();
        var result = await _mediator.Send(query, context.CancellationToken);

        return MapToResponse(result);
    }

    public override async Task<ReserveStockResponse> ReserveStock(
        ReserveStockRequest request,
        ServerCallContext context
    )
    {
        var command = new ReserveStockCommand(
            Guid.Parse(request.OrderId),
            request
                .Items.Select(i => new OrderItemDto(Guid.Parse(i.ProductId), i.Quantity))
                .ToList()
        );

        var result = await _mediator.Send(command, context.CancellationToken);

        return new ReserveStockResponse
        {
            Success = result.IsSuccess,
            FailureReason = result.FailureMessage ?? string.Empty,
            ErrorCode = MapErrorCode(result.ErrorCode),
        };
    }

    public override async Task<ReleaseStockResponse> ReleaseStock(
        ReleaseStockRequest request,
        ServerCallContext context
    )
    {
        var command = new ReleaseStockCommand(Guid.Parse(request.OrderId));
        var result = await _mediator.Send(command, context.CancellationToken);

        return new ReleaseStockResponse
        {
            Success = result.Success,
            FailureReason = result.FailureReason ?? string.Empty,
        };
    }
#pragma warning restore CA1062
}
