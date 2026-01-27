# Task 1: Project Setup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ⚪ pending |
| Dependencies | - |

## Summary
Create Gateway API project with YARP reverse proxy and Aspire integration.

## Scope
- [ ] Add `Yarp.ReverseProxy` package to `Directory.Packages.props`
- [ ] Create `EShop.Gateway.Api` project in `src/Services/Gateway/`
- [ ] Add project reference to solution (`EShopDemo.sln`)
- [ ] Reference `EShop.ServiceDefaults` for Aspire integration
- [ ] Reference `EShop.Common` for CorrelationId middleware
- [ ] Implement basic `Program.cs` with `AddServiceDefaults()` and YARP setup

## Related Specs
- → [internal-api-communication.md](../../high-level-specs/internal-api-communication.md)
- → [aspire-orchestration.md](../../high-level-specs/aspire-orchestration.md)

---
## Notes
(Updated during implementation)
