namespace EShop.Contracts.ServiceClients.Product;

// Protocol-agnostic abstraction
public interface IProductServiceClient
{
    Task<GetProductsResult> GetAllProductsAsync(CancellationToken cancellationToken = default);

    Task<StockReservationResult> ReserveStockAsync(
        ReserveStockRequest request,
        CancellationToken cancellationToken = default
    );

    Task<StockReleaseResult> ReleaseStockAsync(
        ReleaseStockRequest request,
        CancellationToken cancellationToken = default
    );
}
