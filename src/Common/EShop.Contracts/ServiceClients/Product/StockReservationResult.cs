namespace EShop.Contracts.ServiceClients.Product;

public sealed record StockReservationResult(bool Success, string? FailureReason = null);
