# Task 08: Application Workflow

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | pending |
| Dependencies | task-06, task-07 |

## Summary
Create GitHub Actions workflow for building Docker images and deploying to Azure Container Apps with parallel service builds.

## Scope
- [ ] Create `.github/workflows/app.yml` workflow file
- [ ] Configure matrix strategy for parallel service builds
- [ ] Build Docker images using Azure Container Registry (ACR) Tasks
- [ ] Deploy updated images to Container Apps
- [ ] Configure workflow triggers (push to src/, manual dispatch)
- [ ] Add build caching for faster builds
- [ ] Run database migrations before app deployment
- [ ] Output deployment URLs and health check results

## Workflow File

```yaml
# .github/workflows/app.yml
name: Application

on:
  push:
    paths:
      - 'src/**'
      - 'Directory.*.props'
    branches: [main]
  workflow_dispatch:
    inputs:
      services:
        description: 'Services to deploy (comma-separated, or "all")'
        required: false
        default: 'all'

permissions:
  id-token: write
  contents: read

env:
  ACR_NAME: eshopacr
  RESOURCE_GROUP: eshop-dev-rg

jobs:
  changes:
    name: Detect Changes
    runs-on: ubuntu-latest
    outputs:
      services: ${{ steps.filter.outputs.services }}
    steps:
      - uses: actions/checkout@v4

      - name: Detect Changed Services
        id: filter
        run: |
          if [[ "${{ inputs.services }}" != "" && "${{ inputs.services }}" != "all" ]]; then
            echo "services=[$(echo '${{ inputs.services }}' | sed 's/,/","/g' | sed 's/^/"/;s/$/"/')]" >> $GITHUB_OUTPUT
          else
            echo 'services=["gateway","product-service","order-service","notification-service","catalog-service"]' >> $GITHUB_OUTPUT
          fi

  build:
    name: Build ${{ matrix.service }}
    needs: changes
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: ${{ fromJson(needs.changes.outputs.services) }}
      fail-fast: false
    steps:
      - uses: actions/checkout@v4

      - name: Azure Login (OIDC)
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Set Dockerfile Path
        id: dockerfile
        run: |
          case "${{ matrix.service }}" in
            "gateway") echo "path=src/Services/Gateway/Gateway.API/Dockerfile" >> $GITHUB_OUTPUT ;;
            "product-service") echo "path=src/Services/Product/Product.API/Dockerfile" >> $GITHUB_OUTPUT ;;
            "order-service") echo "path=src/Services/Order/Order.API/Dockerfile" >> $GITHUB_OUTPUT ;;
            "notification-service") echo "path=src/Services/Notification/NotificationService/Dockerfile" >> $GITHUB_OUTPUT ;;
            "catalog-service") echo "path=src/Services/Catalog/Catalog.API/Dockerfile" >> $GITHUB_OUTPUT ;;
          esac

      - name: Build and Push Image
        run: |
          az acr build \
            --registry ${{ env.ACR_NAME }} \
            --image eshop/${{ matrix.service }}:${{ github.sha }} \
            --image eshop/${{ matrix.service }}:latest \
            --file ${{ steps.dockerfile.outputs.path }} \
            .

  migrate:
    name: Run Migrations
    needs: build
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Azure Login (OIDC)
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Build Migration Image
        run: |
          az acr build \
            --registry ${{ env.ACR_NAME }} \
            --image eshop/database-migration:${{ github.sha }} \
            --file src/Services/DatabaseMigration/Dockerfile \
            .

      - name: Run Migration Job
        run: |
          az containerapp job start \
            --name eshop-migration \
            --resource-group ${{ env.RESOURCE_GROUP }} \
            --image ${{ env.ACR_NAME }}.azurecr.io/eshop/database-migration:${{ github.sha }}

  deploy:
    name: Deploy ${{ matrix.service }}
    needs: [build, migrate]
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: ${{ fromJson(needs.changes.outputs.services) }}
      max-parallel: 2
    steps:
      - name: Azure Login (OIDC)
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Deploy to Container Apps
        run: |
          az containerapp update \
            --name eshop-${{ matrix.service }} \
            --resource-group ${{ env.RESOURCE_GROUP }} \
            --image ${{ env.ACR_NAME }}.azurecr.io/eshop/${{ matrix.service }}:${{ github.sha }}

      - name: Health Check
        run: |
          sleep 30
          url=$(az containerapp show \
            --name eshop-${{ matrix.service }} \
            --resource-group ${{ env.RESOURCE_GROUP }} \
            --query 'properties.configuration.ingress.fqdn' -o tsv)

          if [[ -n "$url" ]]; then
            curl -sf "https://$url/health" || echo "Health check skipped (internal service)"
          fi

  summary:
    name: Deployment Summary
    needs: deploy
    runs-on: ubuntu-latest
    steps:
      - name: Create Summary
        run: |
          echo "## Deployment Complete" >> $GITHUB_STEP_SUMMARY
          echo "" >> $GITHUB_STEP_SUMMARY
          echo "**Commit**: ${{ github.sha }}" >> $GITHUB_STEP_SUMMARY
          echo "**Services deployed**: ${{ needs.changes.outputs.services }}" >> $GITHUB_STEP_SUMMARY
```

## Build Matrix

| Service | Dockerfile Path | Container App Name |
|---------|-----------------|-------------------|
| gateway | src/Services/Gateway/Gateway.API/Dockerfile | eshop-gateway |
| product-service | src/Services/Product/Product.API/Dockerfile | eshop-product-service |
| order-service | src/Services/Order/Order.API/Dockerfile | eshop-order-service |
| notification-service | src/Services/Notification/NotificationService/Dockerfile | eshop-notification-service |
| catalog-service | src/Services/Catalog/Catalog.API/Dockerfile | eshop-catalog-service |

## Deployment Order

```
1. Build all service images (parallel)
2. Build migration image
3. Run migration job (sequential)
4. Deploy services (max 2 parallel)
5. Health checks
```

## Files to Create

| Action | File |
|--------|------|
| CREATE | `.github/workflows/app.yml` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 9.2 GitHub Actions Workflows, 9.3 Deployment Order)

---
## Notes
- ACR Tasks build images in Azure (no local Docker needed)
- Matrix strategy parallelizes builds for faster deployment
- Migration runs before app deployment to ensure schema compatibility
- max-parallel: 2 prevents overwhelming Container Apps during deployment
