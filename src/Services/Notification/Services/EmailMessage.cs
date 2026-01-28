namespace EShop.NotificationService.Services;

public record EmailMessage(string To, string Subject, string HtmlBody);
