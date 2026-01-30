# Phase 8: Notification Service

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Implement worker service for notification processing using MassTransit consumers with idempotent message handling.

## Scope
- [x] Create Worker Service project (`EShop.NotificationService`)
- [x] Implement Inbox pattern with `ProcessedMessage` entity and `NotificationDbContext`
- [x] Implement `IdempotentConsumer<T>` base class for exactly-once processing
- [x] Implement `FakeEmailService` for simulated email sending (logs instead of actual sending)
- [x] Implement event consumers: `OrderConfirmedConsumer`, `OrderRejectedConsumer`, `OrderCancelledConsumer`, `StockLowConsumer`
- [x] Integrate with Aspire orchestration (PostgreSQL + RabbitMQ)

## Key Implementation Details

### Architecture
- **Worker Service** using `Microsoft.NET.Sdk.Worker`
- **MassTransit** with RabbitMQ transport
- **EF Core** with PostgreSQL for processed messages tracking
- **Inbox Pattern** for idempotent message processing

### Project Structure
```
src/Services/Notification/
├── Configuration/
│   └── NotificationSettings.cs       # Settings with admin email
├── Consumers/
│   ├── IdempotentConsumer.cs         # Base class for all consumers
│   ├── OrderConfirmedConsumer.cs
│   ├── OrderRejectedConsumer.cs
│   ├── OrderCancelledConsumer.cs
│   └── StockLowConsumer.cs
├── Data/
│   ├── Entities/
│   │   └── ProcessedMessage.cs       # Inbox pattern entity
│   ├── Configuration/
│   │   └── ProcessedMessageConfiguration.cs
│   ├── Migrations/
│   └── NotificationDbContext.cs
├── Services/
│   ├── IEmailService.cs
│   ├── EmailMessage.cs
│   ├── EmailResult.cs
│   └── FakeEmailService.cs
├── DependencyInjection.cs
└── Program.cs
```

## Related Specs
- → [messaging-communication.md](../high-level-specs/messaging-communication.md)

---
## Notes
- All 7 tasks completed
- Uses `IDateTimeProvider` from SharedKernel for testability
- Correlation ID propagation via `UseCorrelationIdFilters`
- Consumer retry policy: 1s, 5s, 15s intervals
