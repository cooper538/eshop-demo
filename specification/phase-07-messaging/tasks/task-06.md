# Task 06: MassTransit Bus Outbox in Order Service

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-05 |

## Summary
Configure MassTransit with Entity Framework Bus Outbox for transactional message publishing in Order Service.

## Scope
- [x] Add `MassTransit.EntityFrameworkCore` package to Order.Infrastructure
- [x] Add Outbox entities to `OrderDbContext` (`AddInboxStateEntity`, `AddOutboxMessageEntity`, `AddOutboxStateEntity`)
- [x] Create EF migration for Outbox tables
- [x] Create `MessagingExtensions.AddMessaging<TDbContext>()` helper with Outbox configuration
- [x] Configure MassTransit in Order.Infrastructure via `AddMessaging<OrderDbContext>()`
- [x] Configure RabbitMQ connection via Aspire (`GetConnectionString("messaging")`)
- [x] Add `UseCorrelationIdFilters()` to MassTransit configuration
- [x] Verify AppHost runs and Order Service connects to RabbitMQ

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 5: Outbox Pattern, Section 8.1: Publisher Configuration)

---
## Notes
- Location: `src/Services/Order/Order.Infrastructure/`
- Outbox configured via reusable `MessagingExtensions.AddMessaging<TDbContext>()` in EShop.Common.Infrastructure
- Uses PostgreSQL for outbox storage (`o.UsePostgres()`)
- `UseBusOutbox()` enabled for transactional message publishing
- Message retry: 1s, 5s, 15s intervals
- Endpoint prefix: "order" (kebab-case formatter)
