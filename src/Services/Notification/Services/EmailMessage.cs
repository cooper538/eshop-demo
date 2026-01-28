namespace EShop.NotificationService.Services;

/// <summary>
/// Represents an email message to be sent.
/// </summary>
/// <param name="To">Recipient email address.</param>
/// <param name="Subject">Email subject line.</param>
/// <param name="HtmlBody">HTML content of the email body.</param>
public record EmailMessage(string To, string Subject, string HtmlBody);
