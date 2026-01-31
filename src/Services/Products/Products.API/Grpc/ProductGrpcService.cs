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

    // Null validation suppressed: gRPC framework guarantees non-null request/context parameters.
    // Input validation (GUID format, empty values) is handled by GrpcValidationInterceptor.
#pragma warning disable CA1062
    public override async Task<GetProductsResponse> GetProducts(
        GetProductsRequest request,
        ServerCallContext context
    )
    {
        var requestedIds = request.ProductIds.Select(Guid.Parse).ToList();
        var query = new GetProductsBatchQuery(requestedIds);
        var result = await _mediator.Send(query, context.CancellationToken);

        // ATOMIC: fail if any product not found (per Google AIP-231)
        var foundIds = result.Products.Select(p => p.ProductId).ToHashSet();
        var missingIds = requestedIds.Where(id => !foundIds.Contains(id)).ToList();

        if (missingIds.Count > 0)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    $"Products not found: {string.Join(", ", missingIds)}"
                )
            );
        }

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
