// Module: Networking (VNet + Subnets + NSG)

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

var vnetAddressPrefix = '10.0.0.0/16'
var containerAppsSubnetPrefix = '10.0.0.0/23'
var postgresSubnetPrefix = '10.0.4.0/24'

resource nsg 'Microsoft.Network/networkSecurityGroups@2024-05-01' = {
  name: '${prefix}-nsg'
  location: location
  tags: tags
  properties: {
    securityRules: [
      {
        name: 'AllowHTTPS'
        properties: {
          priority: 100
          direction: 'Inbound'
          access: 'Allow'
          protocol: 'Tcp'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
          destinationAddressPrefix: '*'
          destinationPortRange: '443'
        }
      }
    ]
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2024-05-01' = {
  name: '${prefix}-vnet'
  location: location
  tags: tags
  properties: {
    addressSpace: {
      addressPrefixes: [vnetAddressPrefix]
    }
    subnets: [
      {
        name: 'container-apps'
        properties: {
          addressPrefix: containerAppsSubnetPrefix
          networkSecurityGroup: { id: nsg.id }
          delegations: [
            {
              name: 'Microsoft.App.environments'
              properties: {
                serviceName: 'Microsoft.App/environments'
              }
            }
          ]
        }
      }
      {
        name: 'postgres'
        properties: {
          addressPrefix: postgresSubnetPrefix
          serviceEndpoints: [
            { service: 'Microsoft.Sql' }
          ]
          delegations: [
            {
              name: 'Microsoft.DBforPostgreSQL.flexibleServers'
              properties: {
                serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
              }
            }
          ]
        }
      }
    ]
  }
}

@description('VNet ID')
output vnetId string = vnet.id

@description('Container Apps subnet ID')
output containerAppsSubnetId string = vnet.properties.subnets[0].id

@description('PostgreSQL subnet ID')
output postgresSubnetId string = vnet.properties.subnets[1].id
