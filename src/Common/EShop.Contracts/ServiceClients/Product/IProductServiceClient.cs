namespace EShop.Contracts.ServiceClients.Product;

// Protocol-agnostic abstraction (gRPC/HTTP based on config)
public interface IProductServiceClient
{
    Task<GetProductsResult> GetProductsAsync(
        IReadOnlyList<Guid> productIds,
        CancellationToken cancellationToken = default
    );

    Task<StockReservationResult> ReserveStockAsync(
        ReserveStockRequest request,
        CancellationToken cancellationToken = default
    );

    Task<StockReleaseResult> ReleaseStockAsync(
        ReleaseStockRequest request,
        CancellationToken cancellationToken = default
    );
}
