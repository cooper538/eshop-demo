# Task 4: Email Service

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement simulated email service that logs emails instead of actually sending them.

## Scope
- [x] Create `IEmailService` interface with `SendAsync(EmailMessage, CancellationToken)` method
- [x] Create `EmailMessage` record (To, Subject, HtmlBody)
- [x] Create `EmailResult` record (Success, ErrorMessage) with factory methods
- [x] Implement `FakeEmailService` that logs email details with masked recipient
- [x] Register `IEmailService` as singleton in DI container

## Implementation

### IEmailService Interface
```csharp
public interface IEmailService
{
    Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
```

### EmailMessage Record
```csharp
public record EmailMessage(string To, string Subject, string HtmlBody);
```

### EmailResult Record
```csharp
public record EmailResult(bool Success, string? ErrorMessage = null)
{
    public static EmailResult Ok() => new(true);
    public static EmailResult Fail(string error) => new(false, error);
}
```

### FakeEmailService
```csharp
public class FakeEmailService(ILogger<FakeEmailService> logger) : IEmailService
{
    public Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Simulated email sent. To: {To}, Subject: {Subject}",
            MaskEmail(message.To), message.Subject);
        return Task.FromResult(EmailResult.Ok());
    }

    private static string MaskEmail(string email) { /* masks local part */ }
}
```

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 6.4. Consumer Implementation Example)

---
## Notes
- Named `FakeEmailService` (not `SimulatedEmailService` as originally planned)
- Email addresses are masked in logs for privacy (e.g., `jo***@example.com`)
- Registered as singleton since it's stateless
