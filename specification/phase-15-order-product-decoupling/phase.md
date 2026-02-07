# Phase 15: Order-Product Event Decoupling

## Metadata
| Key | Value |
|-----|-------|
| Status | 🔴 not started |

## Objective
Replace synchronous gRPC `GetProducts` call in Order service with an event-driven local read model (`ProductSnapshot`) synced via integration events, while keeping stock operations (`ReserveStock`, `ReleaseStock`) synchronous via gRPC.

## Scope
- [ ] Publish `ProductCreatedEvent` / `ProductUpdatedEvent` integration events from Product service
- [ ] Create `ProductSnapshot` read model entity + EF configuration + migration in Order service
- [ ] Implement MassTransit consumers for product events in Order service (upsert with timestamp guard)
- [ ] Replace gRPC `GetProducts` call in `CreateOrderCommandHandler` with local `ProductSnapshot` query
- [ ] Add startup sync job for initial `ProductSnapshot` population (cold start scenario)
- [ ] Update unit/integration tests for new data flow
- [ ] Update architecture documentation

## Tasks

| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Publish product integration events](tasks/task-01-publish-product-events.md) | ✅ | - |
| 02 | [ProductSnapshot entity and migration](tasks/task-02-product-snapshot-entity.md) | ✅ | - |
| 03 | [Product event consumers](tasks/task-03-product-event-consumers.md) | ✅ | 02 |
| 04 | [Replace gRPC catalog lookup with local query](tasks/task-04-replace-grpc-catalog-lookup.md) | 🔴 | 02 |
| 05 | [Initial data sync and tests](tasks/task-05-initial-sync-and-tests.md) | 🔴 | 01, 03, 04 |

## Related Specs
- → [messaging-communication.md](../high-level-specs/messaging-communication.md)
- → [grpc-communication.md](../high-level-specs/grpc-communication.md) (stock operations stay synchronous)
- → [order-service-interface.md](../high-level-specs/order-service-interface.md) (CreateOrder flow changes)
- → [product-service-interface.md](../high-level-specs/product-service-interface.md) (event publishing)
- → [unit-testing.md](../high-level-specs/unit-testing.md)

---
## Notes
- Hybrid approach: catalog data via events (eventual consistency), stock operations via gRPC (strong consistency)
- Detailed implementation plan: `PLAN.md` in repository root
