# Task 06: E2E Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ⚪ pending |
| Dependencies | task-04 |

## Summary
Setup end-to-end test infrastructure for multi-service integration testing with Aspire.

**Note**: Shell-based E2E tooling already exists in `/tools/e2e-test/` for manual testing. This task adds programmatic .NET test project for CI integration.

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

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md) (Section: E2E Testing)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md)

---
## Notes
- This is complex - consider using WebApplicationFactory per service
- May need custom service discovery for test environment
- Consider timeouts carefully - E2E tests are slower
- Use async polling for eventual consistency checks
