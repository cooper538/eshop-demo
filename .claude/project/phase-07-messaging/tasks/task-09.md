# Task 09: Product StockLow Event Publishing

## Metadata
| Key | Value |
|-----|-------|
| ID | task-09 |
| Status | 🔵 in_progress |
| Dependencies | task-08 |

## Summary
Implement StockLow integration event publishing from Product Service when stock falls below threshold.

## Scope
- [ ] Update `LowStockWarningEventHandler` (or create if missing) to publish `StockLow` integration event
- [ ] Inject `IPublishEndpoint` into handler
- [ ] Transform domain event data to `StockLow` integration event from `EShop.Contracts`
- [ ] Add appropriate logging
- [ ] Verify event is published to RabbitMQ and received by Notification Service
- [ ] Test end-to-end flow: reserve stock → low stock warning → notification

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section 4.3: Product Events)

---
## Notes
(Updated during implementation)
