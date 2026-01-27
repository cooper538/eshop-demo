# Task 13: E2E Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-13 |
| Status | ⚪ pending |
| Dependencies | task-10 |

## Objective
Setup end-to-end test infrastructure for multi-service integration testing.

## Scope
- [ ] Create `tests/E2E.Tests/` project
- [ ] Add NuGet packages:
  - [ ] WireMock.Net (for external API mocking)
  - [ ] All Testcontainers packages
- [ ] Create `E2ETestFixture` class
  - [ ] Start all required containers (PostgreSQL, RabbitMQ)
  - [ ] Start Gateway, Product, Order, Notification services
  - [ ] Configure service discovery between services
  - [ ] Setup WireMock for SendGrid
- [ ] Create `E2ETestBase` class
  - [ ] HttpClient configured for Gateway
  - [ ] Database reset between tests
  - [ ] Wait utilities for async operations
- [ ] Implement service startup orchestration
  - [ ] Proper startup order
  - [ ] Health check verification before tests
- [ ] Update `EShopDemo.sln`

## Dependencies
- Depends on: task-10
- Blocks: task-14, task-15

## Acceptance Criteria
- [ ] All services start successfully in test environment
- [ ] Services can communicate via configured endpoints
- [ ] WireMock intercepts external API calls
- [ ] Health checks pass before tests run

## Notes
- This is complex - consider using WebApplicationFactory per service
- May need custom service discovery for test environment
- Consider timeouts carefully - E2E tests are slower
- Use async polling for eventual consistency checks
