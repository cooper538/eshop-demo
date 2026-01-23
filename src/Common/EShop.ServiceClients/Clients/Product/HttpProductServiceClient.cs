using EShop.ServiceClients.Clients.Product.Models;
using EShop.ServiceClients.Infrastructure.Http;

namespace EShop.ServiceClients.Clients.Product;

public sealed class HttpProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;

    public HttpProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<StockReservationResult> ReserveStockAsync(
        ReserveStockRequest request,
        CancellationToken cancellationToken = default
    ) =>
        _httpClient.PostAndReadAsync<ReserveStockRequest, StockReservationResult>(
            "internal/products/reserve-stock",
            request,
            cancellationToken
        );

    public Task<StockReleaseResult> ReleaseStockAsync(
        ReleaseStockRequest request,
        CancellationToken cancellationToken = default
    ) =>
        _httpClient.PostAndReadAsync<ReleaseStockRequest, StockReleaseResult>(
            "internal/products/release-stock",
            request,
            cancellationToken
        );
}
