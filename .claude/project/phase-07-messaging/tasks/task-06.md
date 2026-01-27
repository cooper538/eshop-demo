# Task 06: MassTransit Bus Outbox in Order Service

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | 🔵 in_progress |
| Dependencies | task-05 |

## Summary
Configure MassTransit with Entity Framework Bus Outbox for transactional message publishing in Order Service.

## Scope
- [ ] Add `MassTransit.EntityFrameworkCore` package to Order.Infrastructure
- [ ] Add Outbox entities to `OrderDbContext` (`AddInboxStateEntity`, `AddOutboxMessageEntity`, `AddOutboxStateEntity`)
- [ ] Create EF migration for Outbox tables
- [ ] Configure MassTransit in Order.API with `AddEntityFrameworkOutbox<OrderDbContext>` and `UseBusOutbox()`
- [ ] Configure RabbitMQ connection via Aspire (`GetConnectionString("messaging")`)
- [ ] Add `UseCorrelationIdFilters()` to MassTransit configuration
- [ ] Verify AppHost runs and Order Service connects to RabbitMQ

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5: Outbox Pattern, Section 8.1: Publisher Configuration)

---
## Notes
(Updated during implementation)
