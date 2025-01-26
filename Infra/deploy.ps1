# Define variables
$resourceGroup = "Weather-app-rg"
$location = "westeurope"
$bicepFile = "./infra/main.bicep"
$functionAppName = "weatherImageApp-functions"
$publishFolder = "./bin/Release/net8.0/publish"
$zipFilePath = "$publishFolder.zip"

# Login to Azure (if required)
Write-Host "Logging into Azure..."
az login --output none

# Ensure the Resource Group exists
Write-Host "Checking if resource group exists..."
$rg = az group show --name $resourceGroup --query "name" -o tsv 2>$null
if (-not $rg) {
    Write-Host "Creating resource group: $resourceGroup"
    az group create --name $resourceGroup --location $location
} else {
    Write-Host "Resource group $resourceGroup already exists."
}

# Deploy Bicep template to set up infrastructure
Write-Host "Deploying Azure resources using Bicep..."
az deployment group create --resource-group $resourceGroup --template-file $bicepFile --output none

# Confirm Function App exists
Write-Host "Checking if Function App exists..."
$functionApp = az functionapp show --name $functionAppName --resource-group $resourceGroup --query "name" -o tsv 2>$null
if (-not $functionApp) {
    Write-Host "Error: Function App $functionAppName was not deployed correctly."
    exit 1
} else {
    Write-Host "Function App $functionAppName exists and is ready for deployment."
}

# Build the function app
Write-Host "Building the function app..."
dotnet publish WeatherImageFunctionApp.csproj --configuration Release --output $publishFolder

# Ensure previous ZIP file is removed
if (Test-Path $zipFilePath) {
    Remove-Item $zipFilePath -Force
}

# Create ZIP package for deployment
Write-Host "Packaging function app into ZIP..."
Compress-Archive -Path "$publishFolder\*" -DestinationPath $zipFilePath

# Deploy function code to Azure
Write-Host "Deploying function app code..."
az functionapp deployment source config-zip --resource-group $resourceGroup --name $functionAppName --src $zipFilePath

# Restart the function app to apply changes
Write-Host "Restarting function app..."
az functionapp restart --name $functionAppName --resource-group $resourceGroup

# Cleanup ZIP file
Write-Host "Cleaning up temporary files..."
Remove-Item $zipFilePath

# Output Function App URL
$functionAppUrl = "https://${functionAppName}.azurewebsites.net/"
Write-Host "Deployment completed successfully! 🎉"
Write-Host "Your Function App is available at: $functionAppUrl"
