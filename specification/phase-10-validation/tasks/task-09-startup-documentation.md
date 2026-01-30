# Task 09: Project Startup Documentation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-09 |
| Status | ⚪ pending |
| Dependencies | task-07, task-08 |

## Summary
Document how to run and test the EShop Demo project for new developers and reviewers.

**Note**: Basic documentation already exists in root `README.md` (tech stack, architecture, getting started, patterns). This task expands testing documentation and adds troubleshooting.

## Scope
- [ ] Create `docs/testing.md`
  - [ ] Running unit tests (`dotnet test`)
  - [ ] Running integration tests (Testcontainers requirements)
  - [ ] Running E2E tests (manual with `/tools/e2e-test/` scripts)
  - [ ] Test coverage reporting (if applicable)
- [ ] Create `docs/troubleshooting.md`
  - [ ] Common Docker issues
  - [ ] Aspire startup problems
  - [ ] Database migration issues
  - [ ] Service communication failures
- [ ] Enhance `docs/getting-started.md` (or README)
  - [ ] Verify prerequisites section is complete
  - [ ] Add development workflow tips
  - [ ] Add debugging tips (Aspire dashboard, logs)
- [ ] Optional: Add diagrams
  - [ ] Service communication diagram (Mermaid)
  - [ ] Data flow for order creation

## Related Specs
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md)

---
## Notes
- Keep docs concise but complete
- Include troubleshooting section for common issues
- Add diagrams if helpful (Mermaid in markdown)
- Test the docs by following them step-by-step
