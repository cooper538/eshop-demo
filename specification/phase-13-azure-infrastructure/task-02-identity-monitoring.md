# Task 02: Identity and Monitoring Modules

## Metadata
| Key | Value |
|-----|-------|
| ID | task-02 |
| Status | pending |
| Dependencies | task-01 |

## Summary
Create Bicep modules for User-Assigned Managed Identity with RBAC role assignments and Log Analytics Workspace for centralized logging.

## Scope
- [ ] Create `identity.bicep` module with User-Assigned Managed Identity
- [ ] Configure role assignments (Key Vault Secrets User, Service Bus Data Owner, AcrPull)
- [ ] Create `monitoring.bicep` module with Log Analytics Workspace
- [ ] Configure 30-day retention and pay-as-you-go tier
- [ ] Output identity principal ID and workspace ID for use by other modules

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.1 User-Assigned Managed Identity, 7. Monitoring)

---
## Notes
- Single managed identity shared by all Container Apps reduces complexity
- Log Analytics 5 GB/month free ingestion covers demo usage
