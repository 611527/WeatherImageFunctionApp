param location string = resourceGroup().location
param storageAccountName string = 'weathergeneratorstorage'
param functionAppPrefix string = 'weatherimagegenerator'
param existingAppServicePlanName string = 'your-existing-plan-name'


// ✅ Reference existing storage account
resource existingStorageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' existing = {
  name: storageAccountName
}

resource serverFarm 'Microsoft.Web/serverfarms@2021-03-01' existing = {
  name: existingAppServicePlanName
}


// ✅ Function App (Shared for All Functions)
resource functionApp 'Microsoft.Web/sites@2021-02-01' = {
  name: '${functionAppPrefix}-functionapp'  // ✅ Fixed Naming
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: serverFarm.id  // ✅ Links to the Consumption Plan
    siteConfig: {
      appSettings: [
        { name: 'AzureWebJobsStorage', value: existingStorageAccount.listKeys().keys[0].value }  // ✅ Correct Storage Reference
        { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~8' }  // ✅ Uses .NET 8
        { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated' }  // ✅ Ensures correct runtime
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// ✅ Deploy Individual Functions Dynamically
var functionNames = [
  'StartJob'
  'ProcessJob'
  'GenerateImage'
  'FetchResults'
]

resource functionApps 'Microsoft.Web/sites/functions@2021-03-01' = [for functionName in functionNames: {
  parent: functionApp
  name: functionName
  properties: {
    config: {
      bindings: [
        {
          type: functionName == 'StartJob' ? 'httpTrigger' : 'queueTrigger'
          direction: 'in'
          authLevel: functionName == 'StartJob' ? 'anonymous' : ''
          methods: functionName == 'StartJob' ? ['post'] : []
          queueName: functionName != 'StartJob' ? 'start-job-queue' : ''
          connection: 'AzureWebJobsStorage'
        }
      ]
    }
  }
}]

// ✅ Define Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${functionAppPrefix}-appinsights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

// ✅ Reference Existing Storage Services (Queues, Tables, Blobs)
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

// ✅ Reference Existing Queues, Table, and Blob Container (Used Correctly)
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
