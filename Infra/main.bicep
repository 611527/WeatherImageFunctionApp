param location string = resourceGroup().location

var prefix = 'weatherImageApp'
var serverFarmName = '${prefix}sf'
var storageAccountName = 'weathergeneratorstorage'

var functionAppName = '${prefix}-functions'

resource serverFarm 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: serverFarmName
  location: location
  tags: resourceGroup().tags
  sku: {
    tier: 'Consumption'
    name: 'Y1'
  }
  kind: 'elastic'
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  tags: resourceGroup().tags
  kind: 'functionapp'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: serverFarm.id
    siteConfig: { FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated' }
    dependsOn: [serverFarm]
  }
}

resource functionAppSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${functionAppName}/appsettings'
  properties: {
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
    WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
    WEBSITE_RUN_FROM_PACKAGE: '1'
    AzureWebJobsStorage: storageAccountConnectionString
    JobQueueName: 'start-job-queue'
    ProcessImageQueueName: 'process-image-queue'
    BlobContainerName: 'weather-images'
    TableStorageConnectionString: storageAccountConnectionString
    TableName: 'JobStatus'
    unsplashApiKey: 'XrTzEuj1JB6o3mR4N1T5xgnAUN1hnLLso8aSoAHBRV4'
  }
}
