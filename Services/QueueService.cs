using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;

namespace WeatherImageFunctionApp.Services
{
    public class QueueService
    {
        private static readonly string StorageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        public async Task EnqueueMessageAsync(string queueName, object message, ILogger logger)
        {
            try
            {
                if (string.IsNullOrEmpty(StorageConnectionString))
                {
                    logger.LogError("AzureWebJobsStorage connection string is not configured.");
                    throw new InvalidOperationException("Storage connection string is not set.");
                }

                var queueClient = new QueueClient(StorageConnectionString, queueName);
                await queueClient.CreateIfNotExistsAsync();

                var serializedMessage = JsonSerializer.Serialize(message);
                await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedMessage)));

                logger.LogInformation($"Message enqueued to {queueName}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error enqueuing message: {ex.Message}");
                throw;
            }
        }
    }
}
