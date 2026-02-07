# Task 01: Publish Product Integration Events

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ done |
| Dependencies | - |

## Summary
Modify existing Product domain event handlers to publish `ProductCreatedEvent` and `ProductUpdatedEvent` integration events via MassTransit.

## Scope
- [ ] Add `IPublishEndpoint` to `ProductCreatedEventHandler`, publish `ProductCreatedEvent` after stock creation
- [ ] Add `IPublishEndpoint` to `ProductUpdatedEventHandler`, publish `ProductUpdatedEvent` after threshold update
- [ ] Follow existing pattern from `LowStockWarningEventHandler`
- [ ] Verify atomic publish via outbox (domain events fire before `SaveChanges`)

## Related Specs
- → [messaging-communication.md](../../high-level-specs/messaging-communication.md) (Section: Integration Events, Outbox Pattern)
- → [product-service-interface.md](../../high-level-specs/product-service-interface.md) (Section: Domain Events)
- → PLAN.md (Phase 1)

---
## Notes
(Updated during implementation)
