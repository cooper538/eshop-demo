# Phase 3: Product Service Core

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Implement Product Service with domain model, external REST API, internal gRPC API, and stock management.

## Scope
- [x] Create Clean Architecture structure (API, Application, Domain, Infrastructure)
- [x] Implement domain entities (ProductEntity, StockEntity, StockReservationEntity)
- [x] Implement domain events (ProductCreated, ProductUpdated, LowStockWarning, StockReservationExpired)
- [x] Implement CQRS handlers (CreateProduct, UpdateProduct, GetProducts, GetProductById, GetProductsBatch, ReserveStock, ReleaseStock, ExpireReservations)
- [x] Configure EF Core with PostgreSQL + MassTransit Outbox
- [x] Create external REST API endpoints
- [x] Create internal gRPC API (GetProducts batch, ReserveStock, ReleaseStock)
- [x] Add FluentValidation validators
- [x] Configure YAML-based settings with schema validation
- [x] Implement background job for stock reservation expiration

## Tasks
| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Clean Architecture Projects](./tasks/task-01.md) | ✅ completed | - |
| 02 | [Domain Model](./tasks/task-02.md) | ✅ completed | task-01 |
| 03 | [DbContext & EF Core](./tasks/task-03.md) | ✅ completed | task-01, task-02 |
| 04 | [CQRS Queries](./tasks/task-04.md) | ✅ completed | task-01, task-02, task-03 |
| 05 | [CQRS Commands](./tasks/task-05.md) | ✅ completed | task-01, task-02, task-03 |
| 06 | [FluentValidation](./tasks/task-06.md) | ✅ completed | task-01, task-05 |
| 07 | [External REST API](./tasks/task-07.md) | ✅ completed | task-01, task-04, task-05, task-06 |
| 08 | [YAML Configuration](./tasks/task-08.md) | ✅ completed | task-01 |

## Related Specs
- → [product-service-interface.md](../high-level-specs/product-service-interface.md)
- → [error-handling.md](../high-level-specs/error-handling.md)
- → [configuration-management.md](../high-level-specs/configuration-management.md)

---
## Notes
Implementation includes more features than originally scoped:
- Stock management is separated into StockEntity with reservation tracking
- gRPC internal API for inter-service communication
- Domain events for integration with other services
- Background job for automatic reservation expiration
