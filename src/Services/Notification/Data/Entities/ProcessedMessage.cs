namespace EShop.NotificationService.Data.Entities;

// Inbox Pattern tracking entity
public class ProcessedMessage
{
    public Guid MessageId { get; set; }

    public string ConsumerType { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; }
}
