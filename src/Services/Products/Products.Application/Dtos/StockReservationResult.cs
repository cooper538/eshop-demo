namespace Products.Application.Dtos;

public sealed record StockReservationResult(bool Success, string? FailureReason = null)
{
    public static StockReservationResult Succeeded() => new(true);

    public static StockReservationResult Failed(string reason) => new(false, reason);

    public static StockReservationResult AlreadyProcessed(string status) =>
        new(false, $"Reservation already processed with status: {status}");
}
