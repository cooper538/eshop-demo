namespace EShop.NotificationService.Services;

/// <summary>
/// Fake email service that logs emails instead of sending them.
/// Useful for development and testing environments.
/// </summary>
public class FakeEmailService(ILogger<FakeEmailService> logger) : IEmailService
{
    public Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Simulated email sent. To: {To}, Subject: {Subject}",
            message.To,
            message.Subject
        );

        logger.LogDebug("Email body: {HtmlBody}", message.HtmlBody);

        return Task.FromResult(EmailResult.Ok());
    }
}
