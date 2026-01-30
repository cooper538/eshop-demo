# Phase 5: Order Service Core

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_check_mark: completed |

## Objective
Implement Order Service with domain model, lifecycle management, external REST API, and Product Service integration.

## Scope
- [x] Create Clean Architecture structure (API, Application, Domain, Infrastructure)
- [x] Implement Order aggregate with status transitions (Created -> Confirmed/Rejected -> Cancelled)
- [x] Implement OrderItem as owned entity
- [x] Create CQRS handlers (CreateOrder, GetOrder, CancelOrder)
- [x] Configure EF Core with PostgreSQL
- [x] Create external REST API endpoints
- [x] Configure YAML-based settings
- [x] Domain events with integration event publishing (MassTransit)
- [x] Product Service integration (stock reservation/release via gRPC)

## Tasks

| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Clean Architecture Projects](./tasks/task-01.md) | :white_check_mark: | - |
| 02 | [Domain Layer](./tasks/task-02.md) | :white_check_mark: | 01 |
| 03 | [DbContext & EF Core](./tasks/task-03.md) | :white_check_mark: | 01, 02 |
| 04 | [CQRS Queries](./tasks/task-04.md) | :white_check_mark: | 03 |
| 05 | [CreateOrder Command](./tasks/task-05.md) | :white_check_mark: | 02, 03 |
| 06 | [CancelOrder Command](./tasks/task-06.md) | :white_check_mark: | 02, 03 |
| 07 | [External REST API](./tasks/task-07.md) | :white_check_mark: | 04, 05, 06 |
| 08 | [YAML Configuration](./tasks/task-08.md) | :white_check_mark: | 01 |

## Task Dependency Graph

```
task-01 ─────┬───────────────────────────────────────┐
             │                                       │
             ▼                                       │
         task-02                                     │
             │                                       │
             └─────────────┬─────────────────────────┘
                           ▼
                       task-03 ◄─────────────────── task-08
                           │
         ┌─────────────────┼─────────────────┐
         ▼                 ▼                 ▼
     task-04           task-05           task-06
         │                 │                 │
         └─────────────────┼─────────────────┘
                           ▼
                       task-07
```

## Implementation Notes (vs. original plan)

**Implemented beyond original scope:**
- Product Service integration (originally planned for Phase 6) - stock reservation in CreateOrder, stock release in CancelOrder
- Domain events (OrderConfirmed, OrderRejected, OrderCancelled) - originally planned for Phase 7
- Integration event publishing via MassTransit outbox - originally planned for Phase 7
- IUnitOfWork pattern in DbContext

**Result:** Order Service is fully functional with end-to-end flow including stock management.

## Related Specs
- -> [order-service-interface.md](../high-level-specs/order-service-interface.md)
- -> [error-handling.md](../high-level-specs/error-handling.md)

## Reference Implementation
Product Service in `src/Services/Products/` uses identical patterns.

---
## Notes
Phase completed with extended scope - includes Phase 6 (Product integration) and partial Phase 7 (messaging) functionality.
