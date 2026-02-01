# Task 07: Infrastructure Workflow

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | ✅ completed |
| Dependencies | task-01, task-02, task-03, task-04, task-05 |

## Summary
Create GitHub Actions workflow for infrastructure deployment using OIDC authentication (Workload Identity Federation) with no stored secrets.

## Scope
- [x] Create `.github/workflows/infra.yml` workflow file
- [x] Configure OIDC authentication with Azure (Workload Identity Federation)
- [x] Add Bicep validation step (what-if)
- [x] Add Bicep deployment step with parameters
- [x] Configure workflow triggers (push to infra/, manual dispatch)
- [x] Add environment protection rules
- [x] Document required Azure AD app registration setup

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.4 Workload Identity Federation, 9.2 GitHub Actions Workflows)

---
## Notes
- OIDC authentication eliminates need for stored secrets
- What-if step provides preview of changes before deployment
- Required secrets: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
