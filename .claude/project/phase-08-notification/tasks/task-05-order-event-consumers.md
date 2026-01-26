# Task 5: Order Event Consumers

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ⚪ pending |
| Dependencies | task-03, task-04 |

## Summary
Implement consumers for OrderConfirmed, OrderRejected, and OrderCancelled events.

## Scope
- [ ] Create `OrderConfirmedConsumer` extending `IdempotentConsumer<OrderConfirmed>`
  - Send confirmation email with order items and total amount
- [ ] Create `OrderRejectedConsumer` extending `IdempotentConsumer<OrderRejected>`
  - Send rejection email with reason
- [ ] Create `OrderCancelledConsumer` extending `IdempotentConsumer<OrderCancelled>`
  - Send cancellation email with reason
- [ ] Register all consumers in MassTransit configuration
- [ ] Configure retry policy (1s, 5s, 15s intervals)

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 6.4. Consumer Implementation Example)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 4.2. Order Events)
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: 7.1. Consumer Retry Configuration)

---
## Notes
(Updated during implementation)
