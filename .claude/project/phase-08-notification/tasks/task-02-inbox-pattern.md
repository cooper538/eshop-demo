# Task 2: Inbox Pattern

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ⚪ pending |
| Dependencies | task-01 |

## Summary
Implement ProcessedMessage entity and InboxDbContext for idempotent message processing.

## Scope
- [ ] Create `ProcessedMessage` entity with composite key (MessageId, ConsumerType)
- [ ] Create `InboxDbContext` with PostgreSQL configuration
- [ ] Add EF Core migration for ProcessedMessages table
- [ ] Add Aspire PostgreSQL package (`Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`)
- [ ] Register `InboxDbContext` in DI with Aspire connection string injection
- [ ] Add database to AppHost (notificationdb)

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 6. Inbox Pattern)

---
## Notes
(Updated during implementation)
