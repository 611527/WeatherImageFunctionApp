using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace WeatherImageFunctionApp.Functions
{
    public static class StartJob
    {
        private const string QueueName = "start-job-queue";

        [Function("StartJob")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start-job")] HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger("StartJob");

            try
            {
                var queueConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                if (string.IsNullOrEmpty(queueConnectionString))
                {
                    log.LogError("AzureWebJobsStorage connection string is not configured.");
                    var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Storage connection string is not configured.");
                    return errorResponse;
                }

                var jobId = Guid.NewGuid().ToString();

                var queueClient = new QueueClient(queueConnectionString, QueueName);
                await queueClient.CreateIfNotExistsAsync();

                var message = JsonSerializer.Serialize(new { jobId });
                await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));

                log.LogInformation($"Job {jobId} successfully added to the queue.");

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteStringAsync($"{{\"jobId\": \"{jobId}\"}}");
                return response;
            }
            catch (Exception ex)
            {
                log.LogError($"Error in StartJob: {ex.Message}");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while starting the job.");
                return errorResponse;
            }
        }
    }
}
