# Phase 10: Testing & Validation

## Metadata
| Key | Value |
|-----|-------|
| Status | 🔵 in_progress |

## Objective
Demonstrational test coverage (~80%) for core infrastructure and Order Service - showcasing unit, integration, and E2E testing patterns.

## Scope
- [x] Unit test infrastructure and shared test utilities
- [ ] SharedKernel DDD building blocks tests
- [ ] Application behaviors tests (pipeline, correlation, domain events)
- [x] Integration test infrastructure (Testcontainers, Respawn)
- [ ] Order Domain tests (state machine, entities)
- [x] E2E test infrastructure (Aspire.Hosting.Testing)
- [ ] Order Application tests (handlers, validators)
- [ ] Order integration tests (API, DB, messaging)
- [ ] E2E order flow tests (including CorrelationId propagation)
- [ ] Project documentation for developers

## Tasks

| # | Task | Status | Dependencies | Est. Tests |
|---|------|--------|--------------|------------|
| 01 | [Unit Test Infrastructure](./tasks/task-01-unit-test-infrastructure.md) | ✅ | - | - |
| 02 | [SharedKernel Tests](./tasks/task-02-sharedkernel-tests.md) | 🔵 | 01 | ~18 |
| 03 | [Application Behaviors Tests](./tasks/task-03-application-behaviors-tests.md) | ⚪ | 01 | ~17 |
| 04 | [Integration Test Infrastructure](./tasks/task-04-integration-test-infrastructure.md) | ✅ | 01 | - |
| 05 | [Order Domain Tests](./tasks/task-05-order-domain-tests.md) | ✅ | 01 | ~18 |
| 06 | [E2E Test Infrastructure](./tasks/task-06-e2e-test-infrastructure.md) | ✅ | 04 | - |
| 07 | [Order Application Tests](./tasks/task-07-order-application-tests.md) | 🔵 | 01, 05 | ~22 |
| 08 | [Order Integration Tests](./tasks/task-08-order-integration-tests.md) | ⚪ | 04, 05 | ~12 |
| 09 | [E2E Order Flow Tests](./tasks/task-09-e2e-order-flow-tests.md) | ⚪ | 06, 08 | ~11 |
| 10 | [Project Documentation](./tasks/task-10-project-documentation.md) | ⚪ | 09 | - |

**Total estimated tests: ~98**

## Test Coverage Summary

| Area | Project | Tests | Coverage Target |
|------|---------|-------|-----------------|
| SharedKernel | Common.UnitTests | ~18 | Entity, AggregateRoot, ValueObject, Guard |
| Behaviors | Common.UnitTests | ~17 | UnitOfWorkExecutor, CorrelationContext, DomainEventDispatcher |
| Order.Domain | Order.UnitTests | ~18 | OrderEntity state machine, OrderItem |
| Order.Application | Order.UnitTests | ~22 | Handlers, Validators, Queries |
| Integration | Common.IntegrationTests | ~12 | EF Core, MassTransit, Outbox |
| E2E | E2E.Tests | ~11 | Order flows, Gateway routing |

## Related Specs
- [unit-testing.md](../high-level-specs/unit-testing.md)
- [functional-testing.md](../high-level-specs/functional-testing.md)
- [order-service-interface.md](../high-level-specs/order-service-interface.md)

---
## Notes

**Reduced scope:** Tests focus on Order Service only (not Product/Notification) - Order has the richest business logic (state machine, gRPC integration, domain events). Same testing patterns apply to other services.

**Demo purpose:** This is a demonstrational project - goal is to showcase testing patterns, not achieve 100% coverage.
