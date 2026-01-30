# Task 2: Inbox Pattern

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Implement ProcessedMessage entity and NotificationDbContext for idempotent message processing.

## Scope
- [x] Create `ProcessedMessage` entity with composite key (MessageId, ConsumerType)
- [x] Create `NotificationDbContext` with PostgreSQL configuration
- [x] Create `ProcessedMessageConfiguration` with EF Core fluent configuration
- [x] Add EF Core migration for ProcessedMessages table
- [x] Register `NotificationDbContext` in DI with Aspire connection string injection
- [x] Add database to AppHost (notificationdb)

## Implementation

### ProcessedMessage Entity
```csharp
public class ProcessedMessage
{
    public Guid MessageId { get; set; }
    public string ConsumerType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
```

### EF Configuration
```csharp
builder.HasKey(x => new { x.MessageId, x.ConsumerType }); // Composite PK
builder.HasIndex(x => x.ProcessedAt); // Index for cleanup queries
```

### DI Registration
```csharp
builder.AddNpgsqlDbContext<NotificationDbContext>(ResourceNames.Databases.Notification);
```

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 6. Inbox Pattern)

---
## Notes
- DbContext named `NotificationDbContext` (not `InboxDbContext` as originally planned)
- Composite primary key ensures one message processed per consumer type
- ProcessedAt index enables efficient cleanup of old records
