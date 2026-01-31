namespace EShop.Products.Application.Dtos;

public enum EStockReservationError
{
    None,
    InsufficientStock,
    ProductNotFound,
}

public sealed record StockReservationResult(
    bool IsSuccess,
    string? FailureMessage = null,
    EStockReservationError ErrorCode = EStockReservationError.None
)
{
    public static StockReservationResult Succeeded() => new(true);

    public static StockReservationResult Failed(string reason) =>
        new(false, reason, EStockReservationError.InsufficientStock);

    public static StockReservationResult ProductNotFound(string reason) =>
        new(false, reason, EStockReservationError.ProductNotFound);

    public static StockReservationResult AlreadyProcessed(string status) =>
        new(false, $"Reservation already processed with status: {status}");
}
