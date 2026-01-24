# Phase 3: Product Service Core

## Metadata
| Key | Value |
|-----|-------|
| Status | ⚪ pending |

## Objective
Implement Product Service with domain and external REST API

## Scope
- [ ] Create Clean Architecture structure (API, Application, Domain, Infrastructure)
- [ ] Implement domain entities (Product, Category)
- [ ] Implement CQRS handlers (CreateProduct, GetProducts, GetProductById, UpdateProduct)
- [ ] Configure EF Core with PostgreSQL
- [ ] Create external REST API endpoints
- [ ] Add FluentValidation validators
- [ ] Write unit tests for domain and handlers

## Tasks
| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Clean Architecture Projects](./tasks/task-01.md) | ⚪ pending | - |
| 02 | [Domain Model](./tasks/task-02.md) | ⚪ pending | task-01 |
| 03 | [DbContext & EF Core](./tasks/task-03.md) | ⚪ pending | task-01, task-02 |
| 04 | [CQRS Queries](./tasks/task-04.md) | ⚪ pending | task-01, task-02, task-03 |
| 05 | [CQRS Commands](./tasks/task-05.md) | ⚪ pending | task-01, task-02, task-03 |
| 06 | [FluentValidation](./tasks/task-06.md) | ⚪ pending | task-01, task-05 |
| 07 | [External REST API](./tasks/task-07.md) | ⚪ pending | task-01, task-04, task-05, task-06 |
| 08 | [Unit Tests](./tasks/task-08.md) | ⚪ pending | task-02, task-04, task-05 |

## Related Specs
- → [product-service-interface.md](../high-level-specs/product-service-interface.md)
- → [error-handling.md](../high-level-specs/error-handling.md)
- → [unit-testing.md](../high-level-specs/unit-testing.md)

---
## Notes
(Updated during implementation)
