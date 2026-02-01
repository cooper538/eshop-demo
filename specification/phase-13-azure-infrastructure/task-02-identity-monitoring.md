# Task 02: Identity and Monitoring Modules

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | ✅ completed |
| Dependencies | task-01 |

## Summary
Create Bicep modules for User-Assigned Managed Identity with RBAC role assignments and Log Analytics Workspace for centralized logging.

## Scope
- [x] Create `identity.bicep` module with User-Assigned Managed Identity
- [x] Configure role assignments (Key Vault Secrets User)
- [x] Create `monitoring.bicep` module with Log Analytics Workspace
- [x] Configure 30-day retention and pay-as-you-go tier
- [x] Output identity principal ID and workspace ID for use by other modules

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.1 User-Assigned Managed Identity, 7. Monitoring)

---
## Notes
- Single managed identity shared by all Container Apps reduces complexity
- Log Analytics 5 GB/month free ingestion covers demo usage
