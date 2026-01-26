namespace EShop.Contracts.ServiceClients.Product;

/// <summary>
/// Internal API abstraction for Product Service communication.
/// Implementation selected based on configuration (gRPC or HTTP).
/// </summary>
public interface IProductServiceClient
{
    /// <summary>
    /// Reserves stock for the given order items.
    /// </summary>
    Task<StockReservationResult> ReserveStockAsync(
        ReserveStockRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Releases previously reserved stock for an order.
    /// </summary>
    Task<StockReleaseResult> ReleaseStockAsync(
        ReleaseStockRequest request,
        CancellationToken cancellationToken = default
    );
}
