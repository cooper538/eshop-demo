# Phase 13: Azure Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| Status | pending |

## Objective
Create Infrastructure as Code (Bicep) and CI/CD pipelines for deploying the EShop demo to Azure Container Apps with scale-to-zero, cost-optimized configuration.

## Scope
- [ ] Set up `infra/` folder structure with Bicep modules
- [ ] Create identity.bicep (User-Assigned Managed Identity + RBAC)
- [ ] Create monitoring.bicep (Log Analytics Workspace)
- [ ] Create postgres.bicep (Flexible Server B1ms + databases)
- [ ] Create service-bus.bicep (Basic tier namespace + queues)
- [ ] Create key-vault.bicep (Standard tier + connection string secrets)
- [ ] Create acr.bicep (Basic tier Container Registry)
- [ ] Create container-apps.bicep (Environment + 6 Container Apps)
- [ ] Create Dockerfiles for all services
- [ ] Create GitHub Actions workflow for infrastructure deployment (OIDC auth)
- [ ] Create GitHub Actions workflow for application build and deploy

## Tasks

| # | ID | Task | Status | Dependencies |
|---|-----|------|--------|--------------|
| 1 | task-01 | Bicep Project Structure | pending | - |
| 2 | task-02 | Identity and Monitoring Modules | pending | task-01 |
| 3 | task-03 | Data Services Modules | pending | task-02 |
| 4 | task-04 | Key Vault Module | pending | task-03 |
| 5 | task-05 | Container Apps Module | pending | task-02, task-03, task-04 |
| 6 | task-06 | Dockerfiles | pending | - |
| 7 | task-07 | Infrastructure Workflow | pending | task-01, task-02, task-03, task-04, task-05 |
| 8 | task-08 | Application Workflow | pending | task-06, task-07 |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (primary reference)
- -> [messaging-communication.md](../high-level-specs/messaging-communication.md) (Service Bus queue names)

---
## Notes
- Target monthly cost: ~$9-17 (active usage), ~$9 (hibernated with PG stopped)
- All Container Apps use scale-to-zero (min replicas: 0)
- PostgreSQL Flexible Server supports stop/start for additional cost savings
- GitHub Actions uses OIDC (Workload Identity Federation) - no stored secrets
