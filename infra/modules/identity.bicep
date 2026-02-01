// Module: Identity
// Creates User-Assigned Managed Identity and role assignments

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

// TODO: Implement in task-02
// - User-Assigned Managed Identity
// - Role assignments (Key Vault Secrets User)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${prefix}-identity'
  location: location
  tags: tags
}

@description('Managed Identity resource ID')
output identityId string = managedIdentity.id

@description('Managed Identity principal ID')
output identityPrincipalId string = managedIdentity.properties.principalId

@description('Managed Identity client ID')
output identityClientId string = managedIdentity.properties.clientId
