# Phase 13: Azure Infrastructure

## Metadata
| Key | Value |
|-----|-------|
| Status | ✅ completed |

## Objective
Create Infrastructure as Code (Bicep) and CI/CD pipelines for deploying the EShop demo to Azure Container Apps with scale-to-zero, cost-optimized configuration.

## Scope
- [x] Set up `infra/` folder structure with Bicep modules
- [x] Create identity.bicep (User-Assigned Managed Identity + RBAC)
- [x] Create monitoring.bicep (Log Analytics Workspace)
- [x] Create postgres.bicep (Flexible Server B1ms + databases)
- [x] Create rabbitmq.bicep (RabbitMQ on Azure Container Instance - ephemeral)
- [x] Create key-vault.bicep (Standard tier + connection string secrets)
- [x] Create container-apps.bicep (Environment + 5 Container Apps with GHCR)
- [x] Create Dockerfiles for all services
- [x] Create GitHub Actions workflow for infrastructure deployment (OIDC auth)
- [x] Create GitHub Actions workflow for application build and deploy

## Tasks

| # | ID | Task | Status | Dependencies |
|---|-----|------|--------|--------------|
| 1 | task-01 | Bicep Project Structure | ✅ completed | - |
| 2 | task-02 | Identity and Monitoring Modules | ✅ completed | task-01 |
| 3 | task-03 | Data Services Modules | ✅ completed | task-02 |
| 4 | task-04 | Key Vault Module | ✅ completed | task-03 |
| 5 | task-05 | Container Apps Module | ✅ completed | task-02, task-03, task-04 |
| 6 | task-06 | Dockerfiles | ✅ completed | - |
| 7 | task-07 | Infrastructure Workflow | ✅ completed | task-01, task-02, task-03, task-04, task-05 |
| 8 | task-08 | Application Workflow | ✅ completed | task-06, task-07 |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (primary reference)
- -> [messaging-communication.md](../high-level-specs/messaging-communication.md) (RabbitMQ topology)

---
## Notes
- Target monthly cost: ~$9-27 (hibernated to active usage)
- All Container Apps use scale-to-zero (min replicas: 0)
- PostgreSQL and RabbitMQ support stop/start for additional cost savings
- GitHub Actions uses OIDC (Workload Identity Federation) - no stored secrets
- **RabbitMQ on ACI** instead of Service Bus - same config as local dev, full MassTransit support
- **Using GHCR** instead of ACR - simplifies setup, no acr.bicep needed
- **RabbitMQ is ephemeral** - no storage.bicep, no persistent storage (simplified for demo)
