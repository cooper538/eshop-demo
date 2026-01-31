# Azure Infrastructure

## Metadata

| Attribute | Value |
|-----------|-------|
| Scope | Azure cloud deployment infrastructure |
| Target | Scale-to-zero, cost-optimized for demo/portfolio |
| Region | West Europe |
| Monthly Cost | ~$9-17 (active usage) |

---

## 1. Overview

This document describes the Azure infrastructure for deploying the EShop demo microservices. The architecture is optimized for **minimal cost** while demonstrating production-ready patterns.

### 1.1 Design Principles

| Principle | Implementation |
|-----------|----------------|
| **Scale-to-zero** | Container Apps with min replicas: 0 |
| **Serverless-first** | Consumption tier for compute |
| **Security by default** | Managed Identity, RBAC, no secrets in code |
| **Cost awareness** | Free tiers, stop/start for databases |
| **Production patterns** | Same architecture patterns as enterprise deployments |

### 1.2 Architecture Diagram

```
                                    ┌─────────────────────────────┐
                                    │      Azure Entra ID         │
                                    │  ┌───────────┐ ┌──────────┐ │
                                    │  │ API App   │ │ Client   │ │
                                    │  │ Reg       │ │ App Reg  │ │
                                    │  └───────────┘ └──────────┘ │
                                    └──────────────┬──────────────┘
                                                   │ OAuth 2.0
                                                   ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                        CONTAINER APPS ENVIRONMENT                             │
│                         (Consumption, Serverless)                             │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   ┌─────────────────────┐                                                    │
│   │    API Gateway      │◄─── HTTPS (External Ingress)                       │
│   │       (YARP)        │                                                    │
│   └──────────┬──────────┘                                                    │
│              │ Internal HTTP                                                 │
│   ┌──────────┴──────────────────────────────┐                                │
│   │                                         │                                │
│   ▼                                         ▼                                │
│   ┌─────────────┐     gRPC      ┌─────────────┐                              │
│   │   Product   │◄─────────────►│    Order    │                              │
│   │   Service   │               │   Service   │                              │
│   └──────┬──────┘               └──────┬──────┘                              │
│          │                             │                                     │
│          │  ┌──────────────────────────┘                                     │
│          │  │                                                                │
│          ▼  ▼                                                                │
│   ┌─────────────┐               ┌─────────────┐      ┌─────────────┐         │
│   │  Catalog    │               │   Payment   │      │Notification │         │
│   │  Service    │               │   Service   │      │  Service    │         │
│   └─────────────┘               └─────────────┘      └──────┬──────┘         │
│                                                             │                │
│   All services use User-Assigned Managed Identity           │                │
│                                                             │                │
└─────────────────────────────────────────────────────────────┼────────────────┘
                    │                                         │
                    │                                         │
                    ▼                                         ▼
    ┌───────────────────────────┐              ┌───────────────────────────┐
    │   PostgreSQL Flexible     │              │    Service Bus (Basic)    │
    │   Server (B1ms)           │              │    ┌─────────────────┐    │
    │   ┌─────────┐ ┌─────────┐ │              │    │ order-confirmed │    │
    │   │productdb│ │ orderdb │ │              │    │ order-rejected  │    │
    │   └─────────┘ └─────────┘ │              │    │ stock-low       │    │
    │   ┌─────────┐ ┌─────────┐ │              │    │ ...             │    │
    │   │notifydb │ │catalogdb│ │              │    └─────────────────┘    │
    │   └─────────┘ └─────────┘ │              └───────────────────────────┘
    └───────────────────────────┘
                    │
                    ▼
    ┌───────────────────────────┐              ┌───────────────────────────┐
    │     Key Vault (Standard)  │              │   Log Analytics Workspace │
    │   ┌─────────────────────┐ │              │     (Pay-as-you-go)       │
    │   │ Connection strings  │ │              │   ┌─────────────────────┐ │
    │   │ Client secrets      │ │              │   │ 5 GB/month free     │ │
    │   │ API keys            │ │              │   │ 30 day retention    │ │
    │   └─────────────────────┘ │              │   └─────────────────────┘ │
    └───────────────────────────┘              └───────────────────────────┘

    ┌───────────────────────────┐
    │  Container Registry       │
    │     (Basic, 10 GB)        │
    └───────────────────────────┘
```

---

## 2. Compute Resources

### 2.1 Container Apps Environment

| Setting | Value | Rationale |
|---------|-------|-----------|
| Plan | Consumption (Serverless) | Pay per request, no idle cost |
| Workload Profiles | Consumption only | No dedicated compute needed |
| Zone Redundancy | Off | Demo project, HA not required |
| Region | West Europe | Low latency for EU users |

### 2.2 Container Apps

All services share the same configuration:

| Setting | Value |
|---------|-------|
| vCPU | 0.25 |
| Memory | 0.5 Gi |
| Min Replicas | 0 (scale-to-zero) |
| Max Replicas | 2 |

**Ingress Configuration:**

| Service | Ingress Type | Endpoint |
|---------|--------------|----------|
| API Gateway (YARP) | External | HTTPS public |
| Product Service | Internal | HTTP internal only |
| Order Service | Internal | HTTP internal only |
| Catalog Service | Internal | HTTP internal only |
| Payment Service | Internal | HTTP internal only |
| Notification Service | Internal | HTTP internal only |

### 2.3 Free Grant Allowance

Container Apps Consumption tier includes generous free grants:

| Resource | Monthly Free Grant |
|----------|-------------------|
| Requests | 2 million |
| vCPU-seconds | 180,000 |
| Memory GiB-seconds | 360,000 |

**Cost Impact:** With scale-to-zero and typical demo usage, compute cost is effectively $0.

---

## 3. Data Resources

### 3.1 PostgreSQL Flexible Server

| Setting | Value | Rationale |
|---------|-------|-----------|
| Tier | Burstable B1ms | Cheapest option for dev/demo |
| vCPU | 1 | Sufficient for demo load |
| Memory | 2 GB | Adequate for multiple databases |
| Storage | 32 GB | Minimal persistent storage |
| Backup Retention | 7 days | Default, no extra cost |
| High Availability | Off | Not needed for demo |
| Geo-Redundancy | Off | Single region sufficient |

### 3.2 Database Layout

| Database | Service | Tables |
|----------|---------|--------|
| productdb | Product Service | Products, Categories, StockReservations |
| orderdb | Order Service | Orders, OrderItems, OutboxMessages |
| notificationdb | Notification Service | ProcessedMessages (Inbox) |
| catalogdb | Catalog Service | SearchableProducts |

### 3.3 Cost Optimization: Stop/Start

PostgreSQL Flexible Server supports **stop/start** for cost savings:

```bash
# Stop server (pay only storage ~$3.68/month)
az postgres flexible-server stop \
  --resource-group eshop-demo-rg \
  --name eshop-postgres

# Start server before using the demo
az postgres flexible-server start \
  --resource-group eshop-demo-rg \
  --name eshop-postgres
```

| State | Monthly Cost |
|-------|--------------|
| Running (B1ms) | ~$12 |
| Stopped | ~$3.68 (storage only) |

---

## 4. Messaging Resources

### 4.1 Service Bus Namespace

| Setting | Value | Rationale |
|---------|-------|-----------|
| Tier | Basic | Cheapest, queues only |
| Messaging Units | 1 | Minimum for Basic tier |
| Max Message Size | 256 KB | Basic tier limit |

### 4.2 Service Bus Queues

| Queue | Publisher | Consumer | Purpose |
|-------|-----------|----------|---------|
| order-confirmed | Order Service | Notification Service | Order confirmation emails |
| order-rejected | Order Service | Notification Service | Order rejection emails |
| order-cancelled | Order Service | Notification Service | Cancellation notifications |
| stock-low | Product Service | Notification Service | Stock alerts |
| stock-reservation-expired | Product Service | Order Service | TTL cleanup |

### 4.3 Basic Tier Limitations

| Feature | Basic Tier | Standard Tier |
|---------|------------|---------------|
| Queues | Yes | Yes |
| Topics/Subscriptions | No | Yes |
| Sessions | No | Yes |
| Duplicate Detection | No | Yes |
| Price | ~$0.05/month | ~$10/month |

**MassTransit Workaround:** Basic tier supports only queues (not topics). MassTransit uses endpoint conventions to route messages to specific queues, achieving pub/sub semantics.

---

## 5. Security and Identity

### 5.1 User-Assigned Managed Identity

Single managed identity shared by all Container Apps:

```
┌─────────────────────────────────────────────────────────────┐
│              User-Assigned Managed Identity                  │
│                   (eshop-identity)                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│   │ Key Vault    │    │ PostgreSQL   │    │ Service Bus  │  │
│   │ Secrets User │    │ Contributor  │    │ Data Owner   │  │
│   └──────────────┘    └──────────────┘    └──────────────┘  │
│                                                             │
│   ┌──────────────┐    ┌──────────────┐                      │
│   │ ACR Pull     │    │ Storage Blob │                      │
│   │              │    │ Data Reader  │                      │
│   └──────────────┘    └──────────────┘                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 Key Vault

| Setting | Value | Rationale |
|---------|-------|-----------|
| SKU | Standard | Sufficient for demo |
| Soft Delete | Enabled | Default, recommended |
| Purge Protection | Disabled | Allow cleanup for demo |
| Authorization | RBAC | Modern access control |

**Stored Secrets:**

| Secret | Description |
|--------|-------------|
| PostgresConnectionString | Database connection string |
| ServiceBusConnectionString | Messaging connection string |
| SendGridApiKey | Email service API key (optional) |
| EntraClientSecret | OAuth client secret |

### 5.3 Entra ID App Registrations

**API Registration (Resource Server):**

| Setting | Value |
|---------|-------|
| Name | eshop-api |
| Supported Accounts | Single tenant |
| Exposed Scope | api.access |
| App ID URI | api://eshop-api |

**Client Registration (Test Client):**

| Setting | Value |
|---------|-------|
| Name | eshop-client |
| Supported Accounts | Single tenant |
| Redirect URIs | http://localhost:5000/callback |
| Client Secret | Stored in Key Vault |

### 5.4 Workload Identity Federation

GitHub Actions authenticates to Azure using OIDC (no stored secrets):

```yaml
# GitHub Actions workflow
- name: Azure Login
  uses: azure/login@v2
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

| Setting | Value |
|---------|-------|
| Issuer | https://token.actions.githubusercontent.com |
| Subject | repo:USERNAME/eshop-demo:ref:refs/heads/main |
| Audiences | api://AzureADTokenExchange |

---

## 6. Container Registry

### 6.1 Azure Container Registry

| Setting | Value | Rationale |
|---------|-------|-----------|
| SKU | Basic | 10 GB storage, no replication |
| Admin User | Disabled | Use managed identity |
| Public Network | Enabled | Required for GitHub Actions |

### 6.2 Image Naming Convention

```
eshopacr.azurecr.io/eshop/<service>:<tag>

Examples:
eshopacr.azurecr.io/eshop/gateway:1.0.0
eshopacr.azurecr.io/eshop/product-service:1.0.0
eshopacr.azurecr.io/eshop/order-service:1.0.0
```

---

## 7. Monitoring

### 7.1 Log Analytics Workspace

| Setting | Value |
|---------|-------|
| Tier | Pay-as-you-go |
| Free Ingestion | 5 GB/month |
| Retention | 30 days |

### 7.2 Collected Data

| Source | Data Type |
|--------|-----------|
| Container Apps | Application logs, stdout/stderr |
| PostgreSQL | Connection logs, slow queries |
| Service Bus | Operation logs |
| Key Vault | Access logs |

### 7.3 Querying Logs

```kusto
// Container Apps logs
ContainerAppConsoleLogs_CL
| where ContainerAppName_s == "product-service"
| where TimeGenerated > ago(1h)
| order by TimeGenerated desc

// Errors across all services
ContainerAppConsoleLogs_CL
| where Log_s contains "Error" or Log_s contains "Exception"
| summarize count() by ContainerAppName_s, bin(TimeGenerated, 5m)
```

---

## 8. Networking

### 8.1 Traffic Flow

```
Internet
    │
    │ HTTPS (TLS termination at Container Apps)
    ▼
┌─────────────────────────────────────────────────────────────┐
│                 Container Apps Environment                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ API Gateway (External Ingress)                       │   │
│  │ - Rate limiting                                      │   │
│  │ - Request routing                                    │   │
│  │ - CORS                                               │   │
│  └───────────────────────┬──────────────────────────────┘   │
│                          │                                   │
│                          │ Internal HTTP (no TLS needed)     │
│                          ▼                                   │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐  │
│  │ Product Svc    │  │ Order Svc      │  │ Other Svcs     │  │
│  │ (Internal)     │  │ (Internal)     │  │ (Internal)     │  │
│  └────────┬───────┘  └───────┬────────┘  └───────┬────────┘  │
│           │                  │                   │           │
└───────────┼──────────────────┼───────────────────┼───────────┘
            │                  │                   │
            ▼                  ▼                   ▼
    ┌───────────────┐  ┌───────────────┐  ┌───────────────┐
    │  PostgreSQL   │  │  Service Bus  │  │  Key Vault    │
    │  (Private EP) │  │  (Private EP) │  │  (Private EP) │
    └───────────────┘  └───────────────┘  └───────────────┘
```

### 8.2 Service Discovery

Container Apps provides built-in service discovery:

| Service | Internal FQDN |
|---------|---------------|
| Product Service | product-service.internal.{env}.{region}.azurecontainerapps.io |
| Order Service | order-service.internal.{env}.{region}.azurecontainerapps.io |

Services reference each other using simple names that resolve automatically.

### 8.3 Private Endpoints (Optional)

For enhanced security, Azure resources can use private endpoints:

| Resource | Private Endpoint | Cost Impact |
|----------|------------------|-------------|
| PostgreSQL | Optional | +$7/month |
| Service Bus | Optional | +$7/month |
| Key Vault | Optional | +$7/month |

**Recommendation:** Skip private endpoints for demo to minimize cost. Use for production.

---

## 9. Deployment Strategy

### 9.1 Infrastructure as Code

Infrastructure deployed via Bicep/ARM templates:

```
infra/
├── main.bicep                 # Entry point
├── modules/
│   ├── container-apps.bicep   # Container Apps Environment + Apps
│   ├── postgres.bicep         # PostgreSQL Flexible Server
│   ├── service-bus.bicep      # Service Bus Namespace + Queues
│   ├── key-vault.bicep        # Key Vault + Secrets
│   ├── acr.bicep              # Container Registry
│   ├── identity.bicep         # Managed Identity + Role Assignments
│   └── monitoring.bicep       # Log Analytics
└── parameters/
    ├── dev.bicepparam         # Development parameters
    └── prod.bicepparam        # Production parameters
```

### 9.2 GitHub Actions Workflows

**Infrastructure Workflow (infra.yml):**

```yaml
name: Infrastructure

on:
  push:
    paths:
      - 'infra/**'
    branches: [main]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
      - name: Deploy Infrastructure
        uses: azure/arm-deploy@v2
        with:
          resourceGroupName: eshop-demo-rg
          template: ./infra/main.bicep
          parameters: ./infra/parameters/prod.bicepparam
```

**Application Workflow (app.yml):**

```yaml
name: Application

on:
  push:
    paths:
      - 'src/**'
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: [gateway, product-service, order-service, notification-service]
    steps:
      - uses: actions/checkout@v4
      - uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
      - name: Build and Push
        run: |
          az acr build \
            --registry eshopacr \
            --image eshop/${{ matrix.service }}:${{ github.sha }} \
            --file src/Services/${{ matrix.service }}/Dockerfile \
            .
      - name: Deploy to Container Apps
        run: |
          az containerapp update \
            --name ${{ matrix.service }} \
            --resource-group eshop-demo-rg \
            --image eshopacr.azurecr.io/eshop/${{ matrix.service }}:${{ github.sha }}
```

### 9.3 Deployment Order

```
1. Infrastructure (idempotent, can run anytime)
   └── Resource Group
   └── Managed Identity
   └── Log Analytics
   └── Key Vault
   └── PostgreSQL
   └── Service Bus
   └── Container Registry
   └── Container Apps Environment

2. Database Migrations
   └── Run migration container job

3. Application Services
   └── Build and push images
   └── Update Container Apps
```

---

## 10. Cost Summary

### 10.1 Resource Costs

| Resource | Tier | Running | Stopped |
|----------|------|---------|---------|
| Container Apps (5x) | Consumption | ~$0* | ~$0 |
| PostgreSQL | B1ms | ~$12 | ~$3.68 |
| Service Bus | Basic | ~$0.05 | ~$0.05 |
| Key Vault | Standard | ~$0.03 | ~$0.03 |
| Container Registry | Basic | ~$5 | ~$5 |
| Log Analytics | Pay-as-you-go | ~$0** | ~$0 |

*Free grant covers typical demo usage
**5 GB/month free ingestion

### 10.2 Monthly Cost Scenarios

| Scenario | Cost |
|----------|------|
| **Active Development** | ~$17/month |
| **Demo/Showcase** | ~$9/month (stop PG when idle) |
| **Hibernated** | ~$9/month (PG stopped) |

### 10.3 Further Cost Optimization

| Option | Savings | Trade-off |
|--------|---------|-----------|
| Neon Free Tier | -$12 | External PostgreSQL, 0.5 GB storage |
| GitHub Container Registry | -$5 | Different registry, manual setup |
| Stop PG when unused | -$8 | Manual start/stop |

**Minimum Viable Cost:** ~$0-5/month with Neon + GHCR

---

## 11. Aspire Integration

### 11.1 Local vs Azure

| Aspect | Local (Aspire) | Azure |
|--------|----------------|-------|
| PostgreSQL | Docker container | Flexible Server |
| RabbitMQ | Docker container | Service Bus |
| Container Registry | Local images | ACR |
| Service Discovery | Aspire built-in | Container Apps built-in |

### 11.2 Configuration Strategy

Services use the same configuration keys, with values differing per environment:

```yaml
# Local (appsettings.Development.yaml)
ConnectionStrings:
  productdb: "Host=localhost;Database=productdb;..."

# Azure (environment variables from Key Vault)
ConnectionStrings__productdb: "@Microsoft.KeyVault(SecretUri=...)"
```

See [Aspire Hybrid Configuration](./aspire-hybrid-configuration.md) for details.

---

## Related Documents

- [Aspire Orchestration](./aspire-orchestration.md) - Local development setup
- [Aspire Hybrid Configuration](./aspire-hybrid-configuration.md) - Environment configuration
- [Messaging Communication](./messaging-communication.md) - MassTransit/Service Bus integration
- [Configuration Management](./configuration-management.md) - YAML configuration approach
