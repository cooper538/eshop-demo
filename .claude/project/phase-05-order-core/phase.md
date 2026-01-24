# Phase 5: Order Service Core

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Implement Order Service with domain model, state machine, and external REST API.

## Scope
- [ ] Create Clean Architecture structure (API, Application, Domain, Infrastructure)
- [ ] Implement Order aggregate with state machine (Created → Confirmed/Rejected → Cancelled)
- [ ] Implement OrderItem as owned entity
- [ ] Create CQRS handlers (CreateOrder, GetOrder, CancelOrder)
- [ ] Configure EF Core with PostgreSQL
- [ ] Create external REST API endpoints
- [ ] Configure YAML-based settings

## Tasks

| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Clean Architecture Projects](./tasks/task-01.md) | :white_circle: | - |
| 02 | [Domain Layer](./tasks/task-02.md) | :white_circle: | 01 |
| 03 | [DbContext & EF Core](./tasks/task-03.md) | :white_circle: | 01, 02 |
| 04 | [CQRS Queries](./tasks/task-04.md) | :white_circle: | 03 |
| 05 | [CreateOrder Command](./tasks/task-05.md) | :white_circle: | 02, 03 |
| 06 | [CancelOrder Command](./tasks/task-06.md) | :white_circle: | 02, 03 |
| 07 | [External REST API](./tasks/task-07.md) | :white_circle: | 04, 05, 06 |
| 08 | [YAML Configuration](./tasks/task-08.md) | :white_circle: | 01 |

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

## Phase Limitations (by design)
- **NO Product Service integration** - that's phase 6
- **NO messaging/outbox** - that's phase 7
- Orders remain in "Created" state (no automatic Confirm/Reject)
- CancelOrder works but doesn't release stock

## Related Specs
- → [order-service-interface.md](../high-level-specs/order-service-interface.md)
- → [error-handling.md](../high-level-specs/error-handling.md)

## Reference Implementation
Product Service in `src/Services/Products/` uses identical patterns.

---
## Notes
(Updated during implementation)
