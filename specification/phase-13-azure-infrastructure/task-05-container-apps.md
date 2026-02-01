# Task 05: Container Apps Module

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | ✅ completed |
| Dependencies | task-02, task-03, task-04 |

## Summary
Create Bicep module for Container Apps Environment with 5 Container Apps configured for scale-to-zero, using GitHub Container Registry (GHCR).

## Scope
- [x] Create `container-apps.bicep` module with Container Apps Environment
- [x] Configure Consumption workload profile (serverless)
- [x] Create 5 Container Apps: gateway, product-service, order-service, notification-service, catalog-service
- [x] Configure scale-to-zero (min replicas: 0, max: 2)
- [x] Assign User-Assigned Managed Identity to all apps
- [x] Configure internal ingress for services, external for gateway
- [x] Set up environment variables for Key Vault URI and Managed Identity Client ID
- [x] Configure GHCR (ghcr.io) registry credentials

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 2. Compute Resources)

---
## Notes
- Using GHCR instead of ACR - no acr.bicep module needed
- Placeholder image used for initial deployment; real images deployed via CI/CD
- Scale-to-zero provides cost savings when idle
- External ingress on gateway only; all services internal
- DatabaseMigration runs as a job (separate from Container Apps)
