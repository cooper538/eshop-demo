namespace EShop.NotificationService.Services;

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
