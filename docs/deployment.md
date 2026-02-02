# Deployment

Azure deployment using Bicep Infrastructure as Code.

## Infrastructure Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Container Apps                       │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌──────────────────┐  │
│  │ Gateway │ │ Product │ │  Order  │ │ Notification/... │  │
│  └─────────┘ └─────────┘ └─────────┘ └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
         │              │              │
         ▼              ▼              ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│  RabbitMQ   │  │ PostgreSQL  │  │  Key Vault  │
│(Container)  │  │  Flexible   │  │  (Secrets)  │
└─────────────┘  └─────────────┘  └─────────────┘
```

## Bicep Modules

| Module | Resources |
|--------|-----------|
| `main.bicep` | Entry point, orchestrates all modules |
| `container-apps.bicep` | All microservices as Container Apps |
| `container-apps-env.bicep` | Container Apps Environment |
| `postgres.bicep` | PostgreSQL Flexible Server |
| `rabbitmq.bicep` | RabbitMQ on Container Apps |
| `key-vault.bicep` | Azure Key Vault for secrets |
| `identity.bicep` | Managed Identity configuration |
| `monitoring.bicep` | Log Analytics, Application Insights |
| `networking.bicep` | Virtual Network, subnets |

## GitHub Actions Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `infra.yml` | Changes to `infra/` | Deploy infrastructure |
| `app.yml` | Changes to `src/` | Build and deploy services |
| `ci.yml` | All PRs | Build and test |
| `quality.yml` | All PRs | SonarCloud analysis |

## Quick Deploy

### Prerequisites

- Azure subscription
- Azure CLI
- GitHub repository with configured secrets

### Manual Deployment

```bash
# Login to Azure
az login

# Create resource group
az group create -n rg-eshop-demo -l westeurope

# Deploy infrastructure
az deployment group create \
  -g rg-eshop-demo \
  -f infra/main.bicep \
  -p postgresAdminPassword='<password>' \
  -p rabbitmqPassword='<password>'
```

### GitHub Actions (Recommended)

1. Configure Azure AD App Registration with OIDC - see [Azure Setup](azure-setup.md)
2. Add required GitHub secrets
3. Push to `main` branch

## Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Azure AD App Client ID |
| `AZURE_TENANT_ID` | Azure AD Tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID |
| `POSTGRES_ADMIN_PASSWORD` | PostgreSQL admin password |
| `RABBITMQ_PASSWORD` | RabbitMQ password |
| `GHCR_USERNAME` | GitHub username (for container registry) |
| `GHCR_TOKEN` | GitHub PAT with `read:packages` scope |

## Related

- [Azure Setup Guide](azure-setup.md) - Detailed Azure AD and GitHub configuration
- [Aspire Integration](aspire-integration.md) - Local development orchestration
