# Task 04: Replace gRPC Catalog Lookup with Local Query

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ done |
| Dependencies | task-02 |

## Summary
Replace the synchronous gRPC `GetProducts` call in `CreateOrderCommandHandler` with a local `ProductSnapshot` database query.

## Scope
- [ ] Replace gRPC `GetProducts` call with `ProductSnapshots` DB query in `CreateOrderCommandHandler`
- [ ] Update missing products validation to use snapshot count check
- [ ] Update order item creation to use snapshot data instead of gRPC `ProductInfo`
- [ ] Keep `IProductServiceClient` dependency (still needed for `ReserveStock`/`ReleaseStock`)
- [ ] Clean up unused `GetProductsResult` references

## Related Specs
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md) (Section: CreateOrder flow)
- → [grpc-communication.md](../../high-level-specs/grpc-communication.md) (stock operations remain synchronous)
- → PLAN.md (Phase 4 -- exact code replacements and cleanup details)

---
## Notes
(Updated during implementation)
