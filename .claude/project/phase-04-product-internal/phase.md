# Phase 4: Product Service Internal API

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Add internal API layer for gRPC and stock management

## Scope
- [ ] Implement gRPC server (ProductGrpcService)
- [ ] Add StockReservation entity and logic
- [ ] Implement ReserveStock and ReleaseStock operations
- [ ] Implement stock reservation expiration (TTL cleanup)

## Tasks
| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Update product.proto](./tasks/task-01.md) | 🔵 in_progress | - |
| 02 | [StockReservation Domain Entity](./tasks/task-02.md) | ⚪ pending | - |
| 03 | [StockReservation Infrastructure](./tasks/task-03.md) | ⚪ pending | task-02 |
| 04 | [ReserveStock and ReleaseStock Commands](./tasks/task-04.md) | ⚪ pending | task-02, task-03 |
| 05 | [GetProductsBatch Query](./tasks/task-05.md) | ⚪ pending | - |
| 06 | [ProductGrpcService Implementation](./tasks/task-06.md) | ⚪ pending | task-01, task-04, task-05 |
| 07 | [Stock Reservation Expiration Job](./tasks/task-07.md) | ⚪ pending | task-03, task-04 |

## Related Specs
- → [product-service-interface.md](../high-level-specs/product-service-interface.md) (stock operations)
- → [grpc-communication.md](../high-level-specs/grpc-communication.md)
- → [internal-api-communication.md](../high-level-specs/internal-api-communication.md)

---
## Notes
(Updated during implementation)
