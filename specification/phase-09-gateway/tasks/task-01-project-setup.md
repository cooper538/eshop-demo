# Task 1: Project Setup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Create Gateway API project with YARP reverse proxy and Aspire integration.

## Scope
- [x] Add `Yarp.ReverseProxy` package to `Directory.Packages.props`
- [x] Create `Gateway.API` project in `src/Services/Gateway/`
- [x] Add project reference to solution (`EShopDemo.sln`)
- [x] Reference `EShop.ServiceDefaults` for Aspire integration
- [x] Reference `EShop.Common.Api` for CorrelationId middleware
- [x] Implement basic `Program.cs` with `AddServiceDefaults()` and YARP setup

## Related Specs
- → [internal-api-communication.md](../../high-level-specs/internal-api-communication.md)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md)

---
## Notes
(Updated during implementation)
