# Task 03: Azure AD Setup Documentation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ✅ completed |
| Dependencies | - |

## Summary
Document the Azure AD (Entra ID) setup required for obtaining access tokens.

## Scope
- [x] Create `docs/entra-id-setup.md` documentation
- [x] Document API app registration steps (eshop-api)
- [x] Document client app registration steps (eshop-client)
- [x] Document how to obtain access token (curl/Postman example)
- [x] Document required appsettings configuration values
- [x] Document security requirements (RS256 algorithm, token expiration best practices)

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.3 Entra ID App Registrations)

---
## Notes
- App Registrations cannot be fully automated via Bicep
- Manual setup in Azure Portal or via Azure CLI
- Include Postman collection example for token acquisition
- Gateway requires RS256 signed tokens (HS256 and others are rejected)
- Document HSTS implications for local development vs production
