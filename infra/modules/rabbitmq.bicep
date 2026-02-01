// Module: RabbitMQ (internal Container App)

@description('Resource naming prefix')
param prefix string

@description('Azure region')
param location string

@description('Resource tags')
param tags object

@description('Container Apps Environment ID')
param environmentId string

@description('RabbitMQ username')
param rabbitMqUser string

@secure()
param rabbitMqPassword string

resource rabbitMq 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${prefix}-rabbitmq'
  location: location
  tags: tags
  properties: {
    environmentId: environmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 5672
        transport: 'tcp'
        exposedPort: 5672
      }
      secrets: [
        {
          name: 'rabbitmq-password'
          value: rabbitMqPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'rabbitmq'
          image: 'rabbitmq:3-management-alpine'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            { name: 'RABBITMQ_DEFAULT_USER', value: rabbitMqUser }
            { name: 'RABBITMQ_DEFAULT_PASS', secretRef: 'rabbitmq-password' }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

output rabbitMqHost string = rabbitMq.properties.configuration.ingress.fqdn
