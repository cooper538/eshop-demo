# Task 04: Integration Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-04 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Setup shared integration testing infrastructure with Testcontainers, Respawn, and WebApplicationFactory.

## Scope
- [x] Create `tests/Common.IntegrationTests/` project
- [x] Add NuGet packages (Testcontainers, Respawn, Mvc.Testing)
- [x] Create PostgreSQL container fixture
- [x] Create RabbitMQ container fixture
- [x] Create database fixture with Respawn reset
- [x] Create IntegrationTestBase class
- [x] Update solution

## Related Specs
- → [functional-testing.md](../../high-level-specs/functional-testing.md) (Section: Integration Testing)

---
## Notes
- Use postgres:16-alpine and rabbitmq:3-management-alpine images
- Containers shared across tests via CollectionFixture
