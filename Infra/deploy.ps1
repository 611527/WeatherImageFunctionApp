# Define Variables
$resourceGroup = "Weather-app-rg"  # ✅ Existing Resource Group
$location = "westeurope"  # ✅ Define location
$storageAccountName = "weathergeneratorstorage"  # ✅ Use existing storage account
$functionAppPrefix = "weatherimagegenerator"  # ✅ Matches Bicep

# Login to Azure
Write-Host "Logging into Azure..."
az login --use-device-code

# Check if the resource group exists
$rgExists = az group exists --name $resourceGroup
if ($rgExists -eq "false") {
    Write-Host "❌ Resource group '$resourceGroup' does not exist!"
    exit 1
}

# Check if storage account exists
$storageExists = az storage account show --name $storageAccountName --resource-group $resourceGroup --query "name" -o tsv
if (-not $storageExists) {
    Write-Host "❌ Storage account '$storageAccountName' not found in resource group '$resourceGroup'."
    exit 1
}

# Deploy Bicep Template
Write-Host "Deploying Bicep Template..."
az deployment group create --resource-group $resourceGroup --template-file infra/main.bicep --parameters location=$location storageAccountName=$storageAccountName functionAppPrefix=$functionAppPrefix existingAppServicePlanName="WestEuropePlan"

# Check if Function App was successfully created
Write-Host "Checking if Function App exists..."
$functionAppName = "${functionAppPrefix}-functionapp"
$functionExists = az functionapp show --name $functionAppName --resource-group $resourceGroup --query "name" -o tsv
if (-not $functionExists) {
    Write-Host "❌ Function App deployment failed! Verify Bicep deployment."
    exit 1
}

# Retrieve Storage Account Connection String
Write-Host "Retrieving Storage Account Connection String..."
$storageConnectionString = az storage account show-connection-string --name $storageAccountName --resource-group $resourceGroup --query connectionString -o tsv

# Build and Publish Function App
Write-Host "Publishing Azure Function App..."
dotnet publish --configuration Release --output ./publish_output

# Deploy Function Code
Write-Host "Deploying Function Code to Azure..."
az functionapp deployment source config-zip --resource-group $resourceGroup --name $functionAppName --src ./publish_output

Write-Host "✅ Deployment Completed Successfully!"
