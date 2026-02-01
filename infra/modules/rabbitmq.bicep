// Module: RabbitMQ
// Creates RabbitMQ on Azure Container Instance (ephemeral - no persistent storage)

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('RabbitMQ username')
param rabbitMqUser string

@description('RabbitMQ password')
@secure()
param rabbitMqPassword string

resource rabbitMq 'Microsoft.ContainerInstance/containerGroups@2023-05-01' = {
  name: '${prefix}-rabbitmq'
  location: location
  tags: tags
  properties: {
    containers: [
      {
        name: 'rabbitmq'
        properties: {
          image: 'rabbitmq:3-management-alpine'
          resources: {
            requests: {
              cpu: json('0.5')
              memoryInGB: json('1.0')
            }
          }
          ports: [
            { port: 5672, protocol: 'TCP' }
            { port: 15672, protocol: 'TCP' }
          ]
          environmentVariables: [
            { name: 'RABBITMQ_DEFAULT_USER', value: rabbitMqUser }
            { name: 'RABBITMQ_DEFAULT_PASS', secureValue: rabbitMqPassword }
          ]
        }
      }
    ]
    osType: 'Linux'
    restartPolicy: 'Always'
    ipAddress: {
      type: 'Public'
      ports: [
        { port: 5672, protocol: 'TCP' }
        { port: 15672, protocol: 'TCP' }
      ]
    }
  }
}

@description('RabbitMQ public IP')
output rabbitMqIp string = rabbitMq.properties.ipAddress.ip
