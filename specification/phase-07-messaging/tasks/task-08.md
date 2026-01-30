# Task 08: MassTransit in Product Service

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | ✅ completed |
| Dependencies | task-05 |

## Summary
Configure MassTransit with Entity Framework Bus Outbox in Product Service for event publishing.

## Scope
- [x] Add `MassTransit.EntityFrameworkCore` package to Products.Infrastructure
- [x] Add Outbox entities to `ProductDbContext` (`AddInboxStateEntity`, `AddOutboxMessageEntity`, `AddOutboxStateEntity`)
- [x] Create EF migration for Outbox tables
- [x] Configure MassTransit in Products.Infrastructure via `AddMessaging<ProductDbContext>()`
- [x] Configure RabbitMQ connection via Aspire (`GetConnectionString("messaging")`)
- [x] Add `UseCorrelationIdFilters()` to MassTransit configuration
- [x] Verify AppHost runs and Product Service connects to RabbitMQ

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 8.1: Publisher Configuration)

---
## Notes
- Location: `src/Services/Products/Products.Infrastructure/`
- Uses same `MessagingExtensions.AddMessaging<TDbContext>()` as Order Service
- Endpoint prefix: "products" (kebab-case formatter)
- Outbox tables in ProductDbContext for transactional publishing
