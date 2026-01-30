# Task 06: E2E Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-04 |

## Summary
Setup end-to-end test infrastructure for multi-service integration testing with Aspire.

**Note**: Shell-based E2E tooling already exists in `/tools/e2e-test/` for manual testing. This task adds programmatic .NET test project for CI integration.

## Scope
- [x] Create `tests/E2E.Tests/` project
- [x] Add NuGet packages:
  - [x] WireMock.Net (for external API mocking)
  - [x] Aspire.Hosting.Testing (uses Aspire's built-in container management)
- [x] Create `E2ETestFixture` class
  - [x] Start all services via DistributedApplicationTestingBuilder
  - [x] Start Gateway, Product, Order services via Aspire AppHost
  - [x] Automatic service discovery via Aspire
  - [x] Setup WireMock for SendGrid
- [x] Create `E2ETestBase` class
  - [x] HttpClient configured for Gateway
  - [x] WireMock reset between tests
  - [x] Wait utilities for async operations
- [x] Implement service startup orchestration
  - [x] Proper startup order via Aspire
  - [x] Health check verification before tests (WaitForResourceHealthyAsync)
- [x] Update `EShopDemo.sln`

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md) (Section: E2E Testing)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md)

---
## Notes
- This is complex - consider using WebApplicationFactory per service
- May need custom service discovery for test environment
- Consider timeouts carefully - E2E tests are slower
- Use async polling for eventual consistency checks
