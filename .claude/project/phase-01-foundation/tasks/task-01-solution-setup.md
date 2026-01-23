# Task 1: Solution Setup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Upravit existující solution strukturu a build props soubory pro sdílené projekty.

## Scope
- [ ] Aktualizovat `Directory.Build.props` - přidat ImplicitUsings, Nullable, common analyzery
- [ ] Aktualizovat `Directory.Packages.props` - přidat central package management pro:
  - MediatR
  - FluentValidation
  - Polly
  - Grpc.AspNetCore, Grpc.Tools, Google.Protobuf
  - Microsoft.EntityFrameworkCore.*
  - OpenTelemetry.*
- [ ] Vytvořit složkovou strukturu `src/Common/` pro sdílené projekty
- [ ] Přidat sdílené projekty do solution file

## Related Specs
- → [shared-projects.md](../../high-level-specs/shared-projects.md) (Section: 2 - Project Dependency Graph)

---
## Notes
(Updated during implementation)
