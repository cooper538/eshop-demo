using EShop.Common.Correlation;
using EShop.Contracts.ServiceClients.Product;
using EShop.ServiceClients.Clients.Product.Mappers;
using EShop.ServiceClients.Configuration;
using EShop.ServiceClients.Infrastructure.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Options;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product;

public sealed class GrpcProductServiceClient : IProductServiceClient
{
    private readonly GrpcProduct.ProductService.ProductServiceClient _client;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    private readonly ServiceClientOptions _options;

    public GrpcProductServiceClient(
        GrpcProduct.ProductService.ProductServiceClient client,
        ICorrelationIdAccessor correlationIdAccessor,
        IOptions<ServiceClientOptions> options
    )
    {
        _client = client;
        _correlationIdAccessor = correlationIdAccessor;
        _options = options.Value;
    }

    public async Task<StockReservationResult> ReserveStockAsync(
        ReserveStockRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client.ReserveStockAsync(
            request.ToGrpc(),
            headers: new Metadata().WithCorrelationId(_correlationIdAccessor.CorrelationId),
            deadline: DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds),
            cancellationToken: cancellationToken
        );

        return response.ToResult();
    }

    public async Task<StockReleaseResult> ReleaseStockAsync(
        ReleaseStockRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client.ReleaseStockAsync(
            request.ToGrpc(),
            headers: new Metadata().WithCorrelationId(_correlationIdAccessor.CorrelationId),
            deadline: DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds),
            cancellationToken: cancellationToken
        );

        return response.ToResult();
    }
}
