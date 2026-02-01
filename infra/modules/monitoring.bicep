// Module: Monitoring
// Creates Log Analytics Workspace for centralized logging

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${prefix}-logs'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

@description('Log Analytics Workspace resource ID')
output workspaceId string = logAnalytics.id

@description('Log Analytics Workspace customer ID')
output workspaceCustomerId string = logAnalytics.properties.customerId
