# Task 01: Unit Test Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | 🔵 in_progress |
| Dependencies | - |

## Objective
Create shared unit testing infrastructure with common test utilities and base classes.

## Scope
- [ ] Create `tests/Common.UnitTests/` project
- [ ] Add NuGet packages: xUnit, FluentAssertions, Moq, AutoFixture
- [ ] Create `TestBase` class with common setup
- [ ] Add test utilities (builders, factories if needed)
- [ ] Update `EShopDemo.sln` with new test project
- [ ] Verify `dotnet test` runs successfully

## Dependencies
- Depends on: none
- Blocks: task-02, task-03, task-04, task-06, task-08, task-10

## Acceptance Criteria
- [ ] `dotnet build tests/Common.UnitTests` succeeds
- [ ] `dotnet test tests/Common.UnitTests` runs (even with 0 tests)
- [ ] All required NuGet packages are referenced

## Notes
- Use central package management (Directory.Packages.props)
- Consider adding AutoFixture.AutoMoq for easier mocking
- Keep infrastructure minimal - add utilities as needed
