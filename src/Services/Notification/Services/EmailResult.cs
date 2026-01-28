namespace EShop.NotificationService.Services;

public record EmailResult(bool Success, string? ErrorMessage = null)
{
    public static EmailResult Ok() => new(true);

    public static EmailResult Fail(string error) => new(false, error);
}
