# Task 1: Solution Setup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Configure solution structure and build props files for shared projects.

## Scope
- [x] Update `Directory.Build.props`:
  - TargetFramework net10.0
  - ImplicitUsings, Nullable enabled
  - TreatWarningsAsErrors, GenerateDocumentationFile
  - EnforceCodeStyleInBuild with latest-recommended AnalysisLevel
  - Custom EShop.RoslynAnalyzers included for all projects
- [x] Update `Directory.Packages.props` - central package management:
  - MediatR
  - FluentValidation
  - gRPC (Grpc.AspNetCore, Grpc.Net.Client, Grpc.Tools, Google.Protobuf)
  - EF Core (Microsoft.EntityFrameworkCore.*)
  - OpenTelemetry.*
  - MassTransit
  - Aspire
  - YARP
  - Serilog
  - Testing (xUnit, Moq, FluentAssertions, AutoFixture, Testcontainers)
- [x] Create folder structure `src/Common/` for shared projects
- [x] Add shared projects to solution file

## Related Specs
- -> [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 2 - Project Dependency Graph)

---
## Notes
- Added EShop.RoslynAnalyzers project for custom code analysis rules
- UseArtifactsOutput enabled for cleaner build output
