# Phase 7: Messaging Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Set up event-driven communication with guaranteed delivery

## Scope
- [ ] Configure MassTransit with RabbitMQ
- [ ] Implement Outbox pattern in Order Service
- [ ] Implement Inbox pattern in EShop.Common
- [ ] Create OutboxProcessor background service
- [ ] Add integration event publishing (OrderConfirmed, OrderRejected, OrderCancelled)

## Related Specs
- → [messaging-communication.md](../high-level-specs/messaging-communication.md)
- → [correlation-id-flow.md](../high-level-specs/correlation-id-flow.md) (messaging filters)

---
## Notes
(Updated during implementation)
