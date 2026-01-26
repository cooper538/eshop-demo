# Task 6: Stock Event Consumer

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | 🔵 in_progress |
| Dependencies | task-03, task-04 |

## Summary
Implement StockLow consumer for admin notifications when stock falls below threshold.

## Scope
- [ ] Create `StockLowConsumer` extending `IdempotentConsumer<StockLow>`
- [ ] Send admin notification email with ProductId, CurrentQuantity, and Threshold
- [ ] Configure admin email address via `appsettings.json` or environment variable
- [ ] Register consumer in MassTransit configuration
- [ ] Configure retry policy (1s, 5s, 15s intervals)

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 4.3. Product Events)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 2. Integration Events Catalog)

---
## Notes
(Updated during implementation)
