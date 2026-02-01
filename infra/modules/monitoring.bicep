// Module: Monitoring
// Creates Log Analytics Workspace and Application Insights for centralized logging and telemetry

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

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${prefix}-insights'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('Log Analytics Workspace resource ID')
output workspaceId string = logAnalytics.id

@description('Log Analytics Workspace customer ID')
output workspaceCustomerId string = logAnalytics.properties.customerId

@description('Application Insights connection string')
output appInsightsConnectionString string = appInsights.properties.ConnectionString
