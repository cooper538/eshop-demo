# Task 01: Gateway JWT Bearer Setup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ⚪ pending |
| Dependencies | - |

## Summary
Configure JWT Bearer authentication in the API Gateway to validate Azure AD tokens.

## Scope
- [ ] Add `Microsoft.AspNetCore.Authentication.JwtBearer` package
- [ ] Configure JWT Bearer authentication in Gateway Program.cs
- [ ] Add configuration for Azure AD (Authority, Audience)
- [ ] Add appsettings section for AzureAd configuration
- [ ] Add TODO comment in backend services: "// TODO: Add auth when needed - currently trusts Gateway"

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.3 Entra ID App Registrations)

---
## Notes
- Uses Microsoft.Identity.Web for simplified Entra ID integration
- Authority: https://login.microsoftonline.com/{tenant-id}/v2.0
- Audience: api://eshop-api
