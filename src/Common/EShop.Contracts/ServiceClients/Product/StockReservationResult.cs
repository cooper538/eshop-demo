namespace EShop.Contracts.ServiceClients.Product;

public enum EStockReservationErrorCode
{
    None,
    InsufficientStock,
    ProductNotFound,
}

public sealed record StockReservationResult(
    bool Success,
    string? FailureReason = null,
    EStockReservationErrorCode ErrorCode = EStockReservationErrorCode.None
);
