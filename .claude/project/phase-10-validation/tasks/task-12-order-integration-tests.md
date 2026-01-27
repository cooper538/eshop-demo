# Task 12: Order Integration Tests

## Metadata
| Key | Value |
|-----|-------|
| ID | task-12 |
| Status | ⚪ pending |
| Dependencies | task-10, task-07 |

## Objective
Integration tests for Order Service API with mocked Product Service gRPC.

## Scope
- [ ] Create `tests/Order.IntegrationTests/` project
- [ ] Create `OrderApiFactory` (WebApplicationFactory)
  - [ ] Replace DbContext with Testcontainer PostgreSQL
  - [ ] Replace IProductServiceClient with mock
  - [ ] Replace MassTransit with InMemory test harness
  - [ ] Apply migrations on startup
- [ ] Test Order API endpoints
  - [ ] POST /orders - create order (stock available → Confirmed)
  - [ ] POST /orders - create order (stock unavailable → Rejected)
  - [ ] GET /orders/{id} - get order
  - [ ] POST /orders/{id}/cancel - cancel order
  - [ ] Validation error responses (400)
  - [ ] Not found responses (404)
- [ ] Test Order lifecycle
  - [ ] Full lifecycle: Create → Confirm → Cancel
  - [ ] Rejection flow: Create → Reject
- [ ] Test Outbox integration
  - [ ] Verify events published via test harness
  - [ ] OrderConfirmed event on confirmation
  - [ ] OrderCancelled event on cancellation
- [ ] Test error handling
  - [ ] Product service unavailable
  - [ ] Product service timeout

## Dependencies
- Depends on: task-10, task-07
- Blocks: task-14

## Acceptance Criteria
- [ ] All REST endpoints have integration tests
- [ ] Order lifecycle fully tested
- [ ] Outbox events verified via MassTransit test harness
- [ ] Error scenarios covered

## Notes
- Mock IProductServiceClient to control stock responses
- Use MassTransit ITestHarness to verify published events
- Test Polly retry/circuit breaker behavior if possible
