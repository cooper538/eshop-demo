# Task 6: Stock Event Consumer

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-03, task-04 |

## Summary
Implement StockLow consumer for admin notifications when stock falls below threshold.

## Scope
- [x] Create `StockLowConsumer` extending `IdempotentConsumer<StockLowEvent>`
- [x] Send admin notification email with ProductId, ProductName, CurrentQuantity, and Threshold
- [x] Create `NotificationSettings` configuration class with admin email
- [x] Create `EmailSettings` nested class for email-related configuration
- [x] Configure admin email address via YAML configuration with validation
- [x] Register consumer in MassTransit configuration

## Implementation

### StockLowConsumer
```csharp
public sealed class StockLowConsumer(
    NotificationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    IOptions<NotificationSettings> options,
    ILogger<StockLowConsumer> logger
) : IdempotentConsumer<StockLowEvent>(dbContext, dateTimeProvider, logger)
{
    private readonly NotificationSettings _settings = options.Value;

    protected override async Task ProcessMessage(ConsumeContext<StockLowEvent> context)
    {
        var message = context.Message;
        var email = new EmailMessage(
            To: _settings.Email.AdminEmail,
            Subject: $"Low Stock Alert: {message.ProductName}",
            HtmlBody: $"""
            <h1>Low Stock Alert</h1>
            <p><strong>Product:</strong> {message.ProductName}</p>
            <p><strong>Product ID:</strong> {message.ProductId}</p>
            <p><strong>Current Quantity:</strong> {message.CurrentQuantity}</p>
            <p><strong>Threshold:</strong> {message.Threshold}</p>
            """
        );
        await emailService.SendAsync(email, context.CancellationToken);
    }
}
```

### NotificationSettings
```csharp
public class NotificationSettings
{
    public const string SectionName = "Notification";
    [Required] public ServiceInfo Service { get; init; } = new();
    [Required] public EmailSettings Email { get; init; } = new();
}

public class EmailSettings
{
    [Required, EmailAddress, StringLength(320)]
    public string AdminEmail { get; init; } = "admin@eshop.local";
}
```

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 4.3. Product Events)

---
## Notes
- Uses `StockLowEvent` from `EShop.Contracts.IntegrationEvents.Product`
- Admin email configured in `appsettings.notification.yaml` with data annotation validation
- Default admin email: `admin@eshop.local`
