using EShop.Contracts.ServiceClients.Product;
using EShop.ServiceClients.Clients.Product.Mappers;
using EShop.ServiceClients.Configuration;
using Microsoft.Extensions.Options;
using GrpcProduct = EShop.Grpc.Product;

namespace EShop.ServiceClients.Clients.Product;

public sealed class GrpcProductServiceClient : IProductServiceClient
{
    private readonly GrpcProduct.ProductService.ProductServiceClient _client;
    private readonly ServiceClientOptions _options;

    public GrpcProductServiceClient(
        GrpcProduct.ProductService.ProductServiceClient client,
        IOptions<ServiceClientOptions> options
    )
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<StockReservationResult> ReserveStockAsync(
        ReserveStockRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _client.ReserveStockAsync(
            request.ToGrpc(),
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
            deadline: DateTime.UtcNow.AddSeconds(_options.TimeoutSeconds),
            cancellationToken: cancellationToken
        );

        return response.ToResult();
    }
}
