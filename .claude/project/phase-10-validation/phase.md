# Phase 10: Testing & Validation

## Metadata
| Key | Value |
|-----|-------|
| Status | :white_circle: pending |

## Objective
Comprehensive testing across all layers and end-to-end validation

## Tasks

| # | Task | Status | Dependencies |
|---|------|--------|--------------|
| 01 | [Unit Test Infrastructure](./tasks/task-01-unit-test-infrastructure.md) | ⚪ | - |
| 02 | [SharedKernel Tests](./tasks/task-02-sharedkernel-tests.md) | ⚪ | 01 |
| 03 | [EShop.Common Tests](./tasks/task-03-eshop-common-tests.md) | ⚪ | 01 |
| 04 | [Product.UnitTests Project](./tasks/task-04-product-unittests-project.md) | ⚪ | 01 |
| 05 | [Product Domain Tests](./tasks/task-05-product-domain-tests.md) | ⚪ | 04 |
| 06 | [Order.UnitTests Project](./tasks/task-06-order-unittests-project.md) | ⚪ | 01 |
| 07 | [Order Domain Tests](./tasks/task-07-order-domain-tests.md) | ⚪ | 06 |
| 08 | [Notification.UnitTests Project](./tasks/task-08-notification-unittests-project.md) | ⚪ | 01 |
| 09 | [Notification Consumers Tests](./tasks/task-09-notification-consumers-tests.md) | ⚪ | 08 |
| 10 | [Integration Test Infrastructure](./tasks/task-10-integration-test-infrastructure.md) | ⚪ | 01 |
| 11 | [Product Integration Tests](./tasks/task-11-product-integration-tests.md) | ⚪ | 10, 05 |
| 12 | [Order Integration Tests](./tasks/task-12-order-integration-tests.md) | ⚪ | 10, 07 |
| 13 | [E2E Test Infrastructure](./tasks/task-13-e2e-test-infrastructure.md) | ⚪ | 10 |
| 14 | [E2E Order Flow Tests](./tasks/task-14-e2e-order-flow-tests.md) | ⚪ | 13 |
| 15 | [CorrelationId E2E Tests](./tasks/task-15-correlationid-e2e-tests.md) | ⚪ | 13 |
| 16 | [Project Startup Documentation](./tasks/task-16-startup-documentation.md) | ⚪ | 14 |

## Dependency Graph

```
task-01 (Unit Test Infrastructure)
├── task-02 (SharedKernel Tests)
├── task-03 (EShop.Common Tests)
├── task-04 (Product.UnitTests) → task-05 (Product Domain Tests)
├── task-06 (Order.UnitTests) → task-07 (Order Domain Tests)
├── task-08 (Notification.UnitTests) → task-09 (Notification Consumers Tests)
└── task-10 (Integration Test Infrastructure)
    ├── task-11 (Product Integration Tests) [also depends on task-05]
    ├── task-12 (Order Integration Tests) [also depends on task-07]
    └── task-13 (E2E Test Infrastructure)
        ├── task-14 (E2E Order Flow Tests) → task-16 (Documentation)
        └── task-15 (CorrelationId E2E Tests)
```

## Execution Order (Topological)

**Parallel Track A (Unit Tests):**
1. task-01 → task-02, task-03 (parallel)
2. task-01 → task-04 → task-05
3. task-01 → task-06 → task-07
4. task-01 → task-08 → task-09

**Parallel Track B (Integration/E2E):**
1. task-01 → task-10
2. task-10 + task-05 → task-11
3. task-10 + task-07 → task-12
4. task-10 → task-13 → task-14, task-15 (parallel)
5. task-14 → task-16

## Related Specs
- → [functional-testing.md](../high-level-specs/functional-testing.md)
- → [unit-testing.md](../high-level-specs/unit-testing.md)

---
## Notes
(Updated during implementation)
