namespace Products.Application.Dtos;

public sealed record StockReleaseResult(bool Success, string? FailureReason = null)
{
    public static StockReleaseResult Succeeded() => new(true);

    public static StockReleaseResult Failed(string reason) => new(false, reason);
}
