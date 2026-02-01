# Task 03: Azure AD Setup Documentation

## Metadata
| Key | Value |
|-----|-------|
| ID | task-03 |
| Status | ⚪ pending |
| Dependencies | - |

## Summary
Document the Azure AD (Entra ID) setup required for obtaining access tokens.

## Scope
- [ ] Create `docs/entra-id-setup.md` documentation
- [ ] Document API app registration steps (eshop-api)
- [ ] Document client app registration steps (eshop-client)
- [ ] Document how to obtain access token (curl/Postman example)
- [ ] Document required appsettings configuration values

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.3 Entra ID App Registrations)

---
## Notes
- App Registrations cannot be fully automated via Bicep
- Manual setup in Azure Portal or via Azure CLI
- Include Postman collection example for token acquisition
