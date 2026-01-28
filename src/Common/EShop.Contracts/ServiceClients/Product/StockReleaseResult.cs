namespace EShop.Contracts.ServiceClients.Product;

public sealed record StockReleaseResult(bool Success, string? FailureReason = null);
