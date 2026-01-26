namespace EShop.NotificationService.Data.Entities;

/// <summary>
/// Tracks processed messages for idempotency (Inbox Pattern).
/// Composite key (MessageId, ConsumerType) ensures each consumer
/// processes each message exactly once.
/// </summary>
public class ProcessedMessage
{
    /// <summary>
    /// The unique message identifier from the message broker.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// The consumer type that processed this message (e.g., "OrderConfirmedConsumer").
    /// Allows the same message to be processed by multiple different consumers.
    /// </summary>
    public string ConsumerType { get; set; } = string.Empty;

    /// <summary>
    /// When the message was processed.
    /// </summary>
    public DateTime ProcessedAt { get; set; }
}
