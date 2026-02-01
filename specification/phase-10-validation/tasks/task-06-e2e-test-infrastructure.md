# Task 06: E2E Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-06 |
| Status | ✅ completed |
| Dependencies | task-04 |

## Summary
Setup end-to-end test infrastructure for multi-service integration testing with Aspire.

## Scope
- [x] Create `tests/E2E.Tests/` project
- [x] Add NuGet packages (WireMock.Net, Aspire.Hosting.Testing)
- [x] Create E2ETestFixture with Aspire AppHost startup
- [x] Create E2ETestBase with HttpClient and WireMock setup
- [x] Implement service health check verification
- [x] Update solution

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md) (Section: E2E Testing)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md)

---
## Notes
- Shell-based E2E tooling exists in `/tools/e2e-test/` for manual testing
- This task adds programmatic .NET test project for CI integration
- Use async polling for eventual consistency checks
