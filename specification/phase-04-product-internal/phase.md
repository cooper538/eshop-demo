# Phase 4: Product Service Internal API

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Add internal API layer for gRPC and stock management

## Scope
- [x] Implement gRPC server (ProductGrpcService)
- [x] Add StockReservation entity and logic
- [x] Implement ReserveStock and ReleaseStock operations
- [x] Implement stock reservation expiration (TTL cleanup)
- [x] Add domain events for stock operations

## Tasks
| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Update product.proto](./tasks/task-01.md) | ✅ completed | - |
| 02 | [StockReservation Domain Entity](./tasks/task-02.md) | ✅ completed | - |
| 03 | [StockReservation Infrastructure](./tasks/task-03.md) | ✅ completed | task-02 |
| 04 | [ReserveStock and ReleaseStock Commands](./tasks/task-04.md) | ✅ completed | task-02, task-03 |
| 05 | [GetProductsBatch Query](./tasks/task-05.md) | ✅ completed | - |
| 06 | [ProductGrpcService Implementation](./tasks/task-06.md) | ✅ completed | task-01, task-04, task-05 |
| 07 | [Stock Reservation Expiration Job](./tasks/task-07.md) | ✅ completed | task-03, task-04 |
| 08 | [Stock Domain Events](./tasks/task-08.md) | ✅ completed | task-04 |

## Related Specs
- → [product-service-interface.md](../high-level-specs/product-service-interface.md) (stock operations)
- → [grpc-communication.md](../high-level-specs/grpc-communication.md)
- → [internal-api-communication.md](../high-level-specs/internal-api-communication.md)

---
## Notes
(Updated during implementation)
