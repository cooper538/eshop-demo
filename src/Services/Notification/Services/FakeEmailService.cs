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
            MaskEmail(message.To),
            message.Subject
        );

        return Task.FromResult(EmailResult.Ok());
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
        {
            return "***@***";
        }

        var localPart = email[..atIndex];
        var domain = email[atIndex..];
        var visibleChars = Math.Min(2, localPart.Length);

        return $"{localPart[..visibleChars]}***{domain}";
    }
}
