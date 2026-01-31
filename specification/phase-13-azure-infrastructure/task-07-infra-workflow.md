# Task 07: Infrastructure Workflow

## Metadata
| Key | Value |
|-----|-------|
| ID | task-07 |
| Status | pending |
| Dependencies | task-01, task-02, task-03, task-04, task-05 |

## Summary
Create GitHub Actions workflow for infrastructure deployment using OIDC authentication (Workload Identity Federation) with no stored secrets.

## Scope
- [ ] Create `.github/workflows/infra.yml` workflow file
- [ ] Configure OIDC authentication with Azure (Workload Identity Federation)
- [ ] Add Bicep validation step (what-if)
- [ ] Add Bicep deployment step with parameters
- [ ] Configure workflow triggers (push to infra/, manual dispatch)
- [ ] Add environment protection rules
- [ ] Output deployment results and resource URLs
- [ ] Document required Azure AD app registration setup

## Workflow File

```yaml
# .github/workflows/infra.yml
name: Infrastructure

on:
  push:
    paths:
      - 'infra/**'
    branches: [main]
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment to deploy'
        required: true
        default: 'dev'
        type: choice
        options:
          - dev
          - prod

permissions:
  id-token: write # Required for OIDC
  contents: read

env:
  AZURE_LOCATION: westeurope

jobs:
  validate:
    name: Validate Bicep
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Azure Login (OIDC)
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Bicep Build
        run: az bicep build --file infra/main.bicep

      - name: What-If Analysis
        run: |
          az deployment sub what-if \
            --location ${{ env.AZURE_LOCATION }} \
            --template-file infra/main.bicep \
            --parameters infra/parameters/${{ inputs.environment || 'dev' }}.bicepparam

  deploy:
    name: Deploy Infrastructure
    needs: validate
    runs-on: ubuntu-latest
    environment: ${{ inputs.environment || 'dev' }}
    steps:
      - uses: actions/checkout@v4

      - name: Azure Login (OIDC)
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Deploy Bicep
        id: deploy
        run: |
          result=$(az deployment sub create \
            --location ${{ env.AZURE_LOCATION }} \
            --template-file infra/main.bicep \
            --parameters infra/parameters/${{ inputs.environment || 'dev' }}.bicepparam \
            --query 'properties.outputs' -o json)
          echo "outputs=$result" >> $GITHUB_OUTPUT

      - name: Summary
        run: |
          echo "## Deployment Complete" >> $GITHUB_STEP_SUMMARY
          echo "" >> $GITHUB_STEP_SUMMARY
          echo "| Resource | Value |" >> $GITHUB_STEP_SUMMARY
          echo "|----------|-------|" >> $GITHUB_STEP_SUMMARY
          echo "| Gateway URL | $(echo '${{ steps.deploy.outputs.outputs }}' | jq -r '.gatewayUrl.value') |" >> $GITHUB_STEP_SUMMARY
```

## OIDC Setup (Azure AD)

```bash
# Create App Registration for GitHub Actions
az ad app create --display-name "GitHub-EShop-OIDC"

# Create Service Principal
az ad sp create --id <app-id>

# Create Federated Credential
az ad app federated-credential create \
  --id <app-id> \
  --parameters '{
    "name": "github-main",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:USERNAME/eshop-demo:ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# Grant Contributor role
az role assignment create \
  --assignee <service-principal-id> \
  --role Contributor \
  --scope /subscriptions/<subscription-id>
```

## GitHub Secrets Required

| Secret | Description |
|--------|-------------|
| AZURE_CLIENT_ID | App Registration Client ID |
| AZURE_TENANT_ID | Azure AD Tenant ID |
| AZURE_SUBSCRIPTION_ID | Azure Subscription ID |

## Files to Create

| Action | File |
|--------|------|
| CREATE | `.github/workflows/infra.yml` |

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 5.4 Workload Identity Federation, 9.2 GitHub Actions Workflows)

---
## Notes
- OIDC authentication eliminates need for stored secrets
- What-if step provides preview of changes before deployment
- Environment protection rules can require approval for prod
