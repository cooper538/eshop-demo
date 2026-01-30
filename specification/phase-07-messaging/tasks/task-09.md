# Task 09: Product StockLow Event Publishing

## Metadata
| Key | Value |
|-----|-------|
| ID | task-09 |
| Status | ✅ completed |
| Dependencies | task-08 |

## Summary
Implement StockLow integration event publishing from Product Service when stock falls below threshold.

## Scope
- [x] Create `LowStockWarningDomainEvent` in Products.Domain
- [x] Create `LowStockWarningEventHandler` implementing `INotificationHandler<DomainEventNotification<LowStockWarningDomainEvent>>`
- [x] Inject `IPublishEndpoint` and `IProductDbContext` into handler
- [x] Fetch product name from database for integration event
- [x] Transform domain event data to `StockLowEvent` integration event from `EShop.Contracts`
- [x] Add appropriate logging (warning level for low stock alert)
- [x] Verify event is published to RabbitMQ and received by Notification Service
- [x] Test end-to-end flow: reserve stock → low stock warning → notification

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 4.3: Product Events)

---
## Notes
- Location: `src/Services/Products/Products.Application/EventHandlers/LowStockWarningEventHandler.cs`
- Domain event: `LowStockWarningDomainEvent(ProductId, AvailableQuantity, Threshold)`
- Integration event: `StockLowEvent(ProductId, ProductName, CurrentQuantity, Threshold)`
- Handler fetches ProductName from database (not in domain event)
- Handler logs warning for low stock situation
- Notification Service consumes via `StockLowConsumer` with Inbox pattern
