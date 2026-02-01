# Task 05: Container Apps Module

## Metadata
| Key | Value |
|-----|-------|
| ID | task-05 |
| Status | pending |
| Dependencies | task-02, task-03, task-04 |

## Summary
Create Bicep modules for Container Registry and Container Apps Environment with 6 Container Apps configured for scale-to-zero.

## Scope
- [ ] Create `acr.bicep` module for Container Registry (Basic tier)
- [ ] Create `container-apps.bicep` module with Container Apps Environment
- [ ] Configure Consumption workload profile (serverless)
- [ ] Create 6 Container Apps: gateway, product-service, order-service, notification-service, catalog-service, database-migration
- [ ] Configure scale-to-zero (min replicas: 0, max: 2)
- [ ] Assign User-Assigned Managed Identity to all apps
- [ ] Configure internal ingress for services, external for gateway
- [ ] Set up environment variables for Key Vault URI and service URLs

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 2. Compute Resources, 6. Container Registry)

---
## Notes
- Placeholder image used for initial deployment; real images deployed via CI/CD
- Scale-to-zero provides cost savings when idle
- External ingress on gateway only; all services internal
