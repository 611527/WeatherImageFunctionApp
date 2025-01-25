param location string = resourceGroup().location
param storageAccountName string = 'weathergeneratorstorage'
param functionAppName string = 'weatherimagegeneratorfunctionapp' // ✅ Ensures unique function app name

var startJobFunctionName = '${prefix}StartJob'
var processJobFunctionName = '${prefix}ProcessJob'
var generateImageFunctionName = '${prefix}GenerateImage'
var fetchResultsFunctionName = '${prefix}FetchResults'


// ✅ Reference existing storage account
resource existingStorageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' existing = {
  name: storageAccountName
}

// ✅ Declare storage services (required for referencing queues, tables, and blobs)
resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2021-09-01' existing = {
  parent: existingStorageAccount
  name: 'default'
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2021-09-01' existing = {
  parent: existingStorageAccount
  name: 'default'
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2021-09-01' existing = {
  parent: existingStorageAccount
  name: 'default'
}

// ✅ Reference existing queues, table, and blob container
resource queueStartJob 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-09-01' existing = {
  parent: queueService
  name: 'start-job-queue'
}

resource queueProcessImage 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-09-01' existing = {
  parent: queueService
  name: 'process-image-queue'
}

resource tableStorage 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-09-01' existing = {
  parent: tableService
  name: 'JobStatus'
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' existing = {
  parent: blobService
  name: 'weather-images'
}

// ✅ Function App Without ServerFarm (Uses Built-in Consumption Plan)
resource functionApp 'Microsoft.Web/sites@2021-02-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: ''  // ✅ Empty value ensures Consumption Plan is used
    siteConfig: {
  appSettings: [
    { name: 'AzureWebJobsStorage', value: existingStorageAccount.properties.primaryEndpoints.blob }
    { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~8' }
    { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet' }
  ]
}

  }
  identity: {
    type: 'SystemAssigned'
  }
}

// ✅ Define Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${functionAppName}-appinsights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}
