namespace EShop.NotificationService.Services;

/// <summary>
/// Represents the result of an email send operation.
/// </summary>
/// <param name="Success">Whether the email was sent successfully.</param>
/// <param name="ErrorMessage">Error message if the send failed, null otherwise.</param>
public record EmailResult(bool Success, string? ErrorMessage = null)
{
    public static EmailResult Ok() => new(true);

    public static EmailResult Fail(string error) => new(false, error);
}
