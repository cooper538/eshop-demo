using '../main.bicep'

// EShop Demo - Azure deployment parameters
param location = 'westeurope'
param prefix = 'eshop'

// Database credentials (provide via CLI)
param postgresAdminLogin = 'eshop_admin'
param postgresAdminPassword = '' // --parameters postgresAdminPassword=<value>

// RabbitMQ credentials (provide via CLI)
param rabbitMqUser = 'eshop'
param rabbitMqPassword = '' // --parameters rabbitMqPassword=<value>

// GitHub Container Registry (provide via CLI)
param ghcrUsername = '' // --parameters ghcrUsername=<github-username>
param ghcrToken = '' // --parameters ghcrToken=<PAT with read:packages>
