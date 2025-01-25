using System;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace WeatherImageFunctionApp.Services
{
    public class JobStatusService
    {
        private static readonly string StorageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private const string TableName = "JobStatus";

        public async Task<TableEntity> GetJobStatusAsync(string jobId, ILogger logger)
        {
            try
            {
                if (string.IsNullOrEmpty(StorageConnectionString))
                {
                    logger.LogError("AzureWebJobsStorage connection string is not configured.");
                    throw new InvalidOperationException("Storage connection string is not set.");
                }

                var tableClient = new TableClient(StorageConnectionString, TableName);
                var entity = await tableClient.GetEntityIfExistsAsync<TableEntity>(jobId, "status");

                return entity.HasValue ? entity.Value : null;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error retrieving job status: {ex.Message}");
                throw;
            }
        }
        public async Task UpdateJobStatusAsync(string jobId, string stationName, string status, ILogger logger)
        {
            try
            {
                if (string.IsNullOrEmpty(StorageConnectionString))
                {
                    logger.LogError("AzureWebJobsStorage connection string is not configured.");
                    throw new InvalidOperationException("Storage connection string is not set.");
                }

                var tableClient = new TableClient(StorageConnectionString, TableName);
                await tableClient.CreateIfNotExistsAsync();

                var jobStatus = new TableEntity(jobId, stationName)
                {
                    { "Status", status },
                    { "Station", stationName },
                    { "Timestamp", DateTime.UtcNow.ToString("o") }
                };

                await tableClient.UpsertEntityAsync(jobStatus);
                logger.LogInformation($"Job status updated for {stationName}: {status}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error updating job status: {ex.Message}");
                throw;
            }
        }
    }
}
