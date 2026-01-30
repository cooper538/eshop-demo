# Task 05: Order Integration Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ⚪ pending |
| Dependencies | task-03, task-04 |

## Summary
Integration tests for Order Service API with real PostgreSQL, mocked gRPC, and MassTransit test harness.

## Scope
- [ ] Create `tests/Order.IntegrationTests/` project
- [ ] Create `OrderApiFactory` (WebApplicationFactory)
  - [ ] Replace DbContext with Testcontainer PostgreSQL
  - [ ] Replace IProductServiceClient with mock
  - [ ] Replace MassTransit with InMemory test harness
  - [ ] Apply migrations on startup
- [ ] Test Order API endpoints
  - [ ] POST /orders - create order (stock available → Confirmed)
  - [ ] POST /orders - create order (stock unavailable → 500 error, reservation fails)
  - [ ] GET /orders/{id} - get order
  - [ ] POST /orders/{id}/cancel - cancel order
  - [ ] Validation error responses (400)
  - [ ] Not found responses (404)
- [ ] Test Order lifecycle
  - [ ] Full lifecycle: Create → Confirm → Cancel
  - [ ] Note: No separate rejection flow - stock failure throws exception
- [ ] Test Outbox integration
  - [ ] Verify events published via test harness
  - [ ] OrderConfirmed event on confirmation
  - [ ] OrderCancelled event on cancellation
- [ ] Test error handling
  - [ ] Product service unavailable
  - [ ] Product service timeout

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md) (Section: Integration Testing)
- → [order-service-interface.md](../../high-level-specs/order-service-interface.md)

---
## Notes
- Mock IProductServiceClient to control stock responses
- Use MassTransit ITestHarness to verify published events
- Test Polly retry/circuit breaker behavior if possible
- **Implementation note**: Stock unavailable = exception thrown (not Order rejection)
