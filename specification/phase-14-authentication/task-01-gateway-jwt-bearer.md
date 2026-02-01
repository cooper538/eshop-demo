# Task 01: Gateway JWT Bearer Setup

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Configure JWT Bearer authentication in the API Gateway to validate Azure AD tokens.

## Scope
- [x] Add `Microsoft.AspNetCore.Authentication.JwtBearer` package
- [x] Configure JWT Bearer authentication in Gateway Program.cs
- [x] Add configuration for Azure AD (Authority, Audience)
- [x] Add appsettings section for AzureAd configuration
- [x] Add TODO comment in backend services: "// TODO: Add auth when needed - currently trusts Gateway"
- [x] Add explicit TokenValidationParameters (security hardening)
- [x] Add algorithm whitelist (RS256 only - prevents algorithm confusion attacks)
- [x] Add security event logging (OnAuthenticationFailed, OnTokenValidated, OnChallenge, OnForbidden)
- [x] Add HSTS for production (365 days, subdomains, preload)

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.3 Entra ID App Registrations)

---
## Notes
- Uses Microsoft.Identity.Web for simplified Entra ID integration
- Authority: https://login.microsoftonline.com/{tenant-id}/v2.0
- Audience: api://eshop-api

### Security Hardening Applied
- **Algorithm whitelist**: Only RS256 allowed (prevents algorithm confusion attacks)
- **Token validation**: Explicit validation of issuer, audience, lifetime, signing key
- **Security logging**: All auth events logged via Serilog for SIEM integration
- **HSTS**: Strict-Transport-Security header enforced in production
