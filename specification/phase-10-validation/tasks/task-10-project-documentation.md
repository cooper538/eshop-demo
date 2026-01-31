# Task 10: Project Documentation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-10 |
| Status | ⚪ pending |
| Dependencies | task-09 |

## Summary
Document testing infrastructure and provide troubleshooting guide for developers.

## Scope

### docs/testing.md
- [ ] Overview of test projects and their purpose
- [ ] Running unit tests (`dotnet test --filter "Category=Unit"`)
- [ ] Running integration tests (Testcontainers requirements, Docker)
- [ ] Running E2E tests (Aspire startup, WireMock)
- [ ] Manual E2E testing with `/tools/e2e-test/` scripts
- [ ] Test naming conventions and patterns used
- [ ] Adding new tests - guidelines

### docs/troubleshooting.md
- [ ] Docker issues (containers not starting, port conflicts)
- [ ] Aspire startup problems (health check failures, timeouts)
- [ ] Database migration issues
- [ ] RabbitMQ connection failures
- [ ] Test failures debugging tips

### README.md updates
- [ ] Add testing section (link to docs/testing.md)
- [ ] Update "Intentionally Omitted" section (tests are now implemented)

## Deliverables

```markdown
# docs/testing.md structure

## Overview
Brief description of testing strategy and coverage goals.

## Test Projects
| Project | Type | Framework | Purpose |
|---------|------|-----------|---------|
| Common.UnitTests | Unit | xUnit + AutoFixture | SharedKernel, Behaviors |
| Order.UnitTests | Unit | xUnit + AutoFixture | Order domain, handlers |
| Common.IntegrationTests | Integration | xUnit + Testcontainers | DB, messaging |
| E2E.Tests | E2E | xUnit + Aspire.Testing | Full service flows |
| EShop.ArchitectureTests | Architecture | MSTest + NetArchTest | Layer rules |

## Running Tests
Commands and prerequisites for each test type.

## Writing New Tests
Guidelines for adding tests to each project.
```

## Related Specs
- [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md)

---
## Notes
- Keep documentation concise and practical
- Include actual commands that work
- Add troubleshooting for issues encountered during implementation
- Update root README.md to reflect completed test infrastructure
