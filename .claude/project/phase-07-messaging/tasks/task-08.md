# Task 08: MassTransit in Product Service

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | 🔵 in_progress |
| Dependencies | task-05 |

## Summary
Configure MassTransit with Entity Framework Bus Outbox in Product Service for event publishing.

## Scope
- [ ] Add `MassTransit.EntityFrameworkCore` package to Products.Infrastructure
- [ ] Add Outbox entities to `ProductDbContext` (`AddInboxStateEntity`, `AddOutboxMessageEntity`, `AddOutboxStateEntity`)
- [ ] Create EF migration for Outbox tables
- [ ] Configure MassTransit in Products.API with `AddEntityFrameworkOutbox<ProductDbContext>` and `UseBusOutbox()`
- [ ] Configure RabbitMQ connection via Aspire (`GetConnectionString("messaging")`)
- [ ] Add `UseCorrelationIdFilters()` to MassTransit configuration
- [ ] Verify AppHost runs and Product Service connects to RabbitMQ

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 8.1: Publisher Configuration)

---
## Notes
(Updated during implementation)
