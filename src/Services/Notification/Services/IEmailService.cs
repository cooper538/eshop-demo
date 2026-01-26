namespace EShop.NotificationService.Services;

/// <summary>
/// Service for sending email notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default
    );
}
