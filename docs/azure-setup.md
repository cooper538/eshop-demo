# Azure Setup Guide

This guide describes how to configure Azure for GitHub Actions deployment using Workload Identity Federation (OIDC).

## Prerequisites

- Azure subscription
- GitHub repository
- Azure CLI installed locally

## 1. Create Azure AD App Registration

```bash
# Login to Azure
az login

# Create app registration
az ad app create --display-name "eshop-github-actions"

# Note the appId (Client ID) from the output
APP_ID=$(az ad app list --display-name "eshop-github-actions" --query "[0].appId" -o tsv)
echo "Client ID: $APP_ID"

# Create service principal
az ad sp create --id $APP_ID

# Get the object ID for role assignment
SP_OBJECT_ID=$(az ad sp show --id $APP_ID --query "id" -o tsv)
```

## 2. Configure Federated Credentials

Create federated credential for GitHub Actions:

```bash
# For main branch deployments
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "github-main-branch",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:YOUR_GITHUB_USERNAME/eshop-demo:ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# For production environment (required for protected deployments)
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "github-production-env",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:YOUR_GITHUB_USERNAME/eshop-demo:environment:production",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

Replace `YOUR_GITHUB_USERNAME/eshop-demo` with your actual repository path.

## 3. Assign Azure Roles

Grant the service principal permissions to deploy resources:

```bash
SUBSCRIPTION_ID=$(az account show --query "id" -o tsv)

# Contributor role at subscription level (for resource group creation)
az role assignment create \
  --assignee $SP_OBJECT_ID \
  --role "Contributor" \
  --scope "/subscriptions/$SUBSCRIPTION_ID"

# User Access Administrator (for role assignments in Bicep)
az role assignment create \
  --assignee $SP_OBJECT_ID \
  --role "User Access Administrator" \
  --scope "/subscriptions/$SUBSCRIPTION_ID"
```

## 4. Configure GitHub Secrets

Add the following secrets to your GitHub repository:

| Secret | Description | How to Get |
|--------|-------------|------------|
| `AZURE_CLIENT_ID` | App Registration Client ID | `az ad app list --display-name "eshop-github-actions" --query "[0].appId" -o tsv` |
| `AZURE_TENANT_ID` | Azure AD Tenant ID | `az account show --query "tenantId" -o tsv` |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID | `az account show --query "id" -o tsv` |
| `POSTGRES_ADMIN_PASSWORD` | PostgreSQL admin password | Generate a strong password |
| `RABBITMQ_PASSWORD` | RabbitMQ password | Generate a strong password |
| `GHCR_USERNAME` | GitHub username | Your GitHub username |
| `GHCR_TOKEN` | GitHub PAT | Create PAT with `read:packages` scope |

### Generate Secure Passwords

```bash
# Generate random password
openssl rand -base64 24
```

### Create GitHub PAT for GHCR

1. Go to GitHub Settings > Developer settings > Personal access tokens > Tokens (classic)
2. Generate new token with `read:packages` scope
3. Save as `GHCR_TOKEN` secret

## 5. Create GitHub Environment

1. Go to repository Settings > Environments
2. Create `production` environment
3. Configure protection rules:
   - Required reviewers (optional)
   - Deployment branches: `main` only

## 6. Verify Setup

Run the infrastructure workflow manually:

```bash
gh workflow run infra.yml
```

Or push a change to the `infra/` directory.

## Security Best Practices

1. **Use environments** - Protect production deployments with required reviewers
2. **Limit federated credentials** - Only create credentials for needed branches/environments
3. **Minimal permissions** - Consider resource group scope instead of subscription for production
4. **Audit logs** - Enable Azure AD sign-in logs for monitoring

## Related Links

- [GitHub Docs: OIDC with Azure](https://docs.github.com/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-azure)
- [Azure Docs: Workload Identity Federation](https://learn.microsoft.com/en-us/azure/active-directory/develop/workload-identity-federation)
- [Azure Docs: Deploy Bicep with GitHub Actions](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/deploy-github-actions)
