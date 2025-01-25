using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WeatherImageFunctionApp.Models;
using WeatherImageFunctionApp.Services;

namespace WeatherImageFunctionApp.Functions
{
    public class StartJobProcessor
    {
        private readonly QueueService _queueService;
        private readonly JobStatusService _jobStatusService;
        private readonly WeatherService _weatherService;

        public StartJobProcessor()
        {
            _queueService = new QueueService();
            _jobStatusService = new JobStatusService();
            _weatherService = new WeatherService();
        }

        [Function("StartJobProcessor")]
        public async Task Run(
            [QueueTrigger("start-job-queue", Connection = "AzureWebJobsStorage")] string queueMessage,
            FunctionContext context)
        {
            var log = context.GetLogger("StartJobProcessor");

            try
            {
                var jobData = JsonSerializer.Deserialize<Dictionary<string, string>>(queueMessage);
                if (jobData == null || !jobData.ContainsKey("jobId"))
                {
                    log.LogError("Invalid queue message. 'jobId' is missing.");
                    return;
                }

                var jobId = jobData["jobId"];
                log.LogInformation($"Processing job ID: {jobId}");

                var weatherData = await _weatherService.FetchWeatherDataAsync(log);
                if (weatherData == null || !weatherData.Any())
                {
                    log.LogError("No weather data available.");
                    return;
                }

                foreach (var station in weatherData)
                {
                    if (string.IsNullOrWhiteSpace(station.Name) || string.IsNullOrWhiteSpace(station.Temperature))
                    {
                        log.LogWarning("Skipping station with invalid data.");
                        continue;
                    }

                    var message = new WeatherJobMessage
                    {
                        JobId = jobId,
                        StationName = station.Name,
                        Temperature = station.Temperature
                    };

                    await _queueService.EnqueueMessageAsync("process-image-queue", message, log);
                    await _jobStatusService.UpdateJobStatusAsync(jobId, station.Name, "Processing", log);

                    log.LogInformation($"Queued image generation for station: {station.Name}");
                }

                log.LogInformation($"Job {jobId} processed. All stations added to process-image-queue.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error in StartJobProcessor: {ex.Message}");
            }
        }
    }
}
