using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WeatherImageFunctionApp.Helpers;
using WeatherImageFunctionApp.Models;
using WeatherImageFunctionApp.Services;

namespace WeatherImageFunctionApp.Functions
{
    public class ProcessImageQueue
    {
        private readonly ImageProcessingService _imageProcessingService;
        private readonly BlobStorageService _blobStorageService;

        public ProcessImageQueue()
        {
            _imageProcessingService = new ImageProcessingService();
            _blobStorageService = new BlobStorageService();
        }

        [Function("ProcessImageQueue")]
        public async Task Run(
            [QueueTrigger("process-image-queue", Connection = "AzureWebJobsStorage")] string queueMessage,
            FunctionContext context)
        {
            var log = context.GetLogger("ProcessImageQueue");

            try
            {
                var jobData = JsonSerializer.Deserialize<WeatherJobMessage>(queueMessage);
                if (jobData == null || string.IsNullOrWhiteSpace(jobData.JobId) ||
                    string.IsNullOrWhiteSpace(jobData.StationName) || string.IsNullOrWhiteSpace(jobData.Temperature))
                {
                    log.LogError("Invalid message format. Missing required fields.");
                    return;
                }

                log.LogInformation($"Processing job ID: {jobData.JobId} for station: {jobData.StationName}");

                var imageStream = await _imageProcessingService.GenerateImageAsync(jobData.StationName, jobData.Temperature, log);
                if (imageStream == null)
                {
                    log.LogError($"Image generation failed for station: {jobData.StationName}");
                    return;
                }

                var sanitizedStationName = string.Concat((jobData.StationName ?? "UnknownStation").Where(char.IsLetterOrDigit));
                var blobName = $"{jobData.JobId}/{sanitizedStationName}.png";

                await _blobStorageService.UploadImageAsync(blobName, imageStream, log);

                log.LogInformation($"Job {jobData.JobId} processed successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error in ProcessImageQueue: {ex.Message}");
            }
        }
    }
}
