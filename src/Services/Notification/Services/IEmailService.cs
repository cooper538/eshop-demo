namespace EShop.NotificationService.Services;

public interface IEmailService
{
    Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default
    );
}
