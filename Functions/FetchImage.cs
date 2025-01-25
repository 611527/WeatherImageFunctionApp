using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WeatherImageFunctionApp.Services;

namespace WeatherImageFunctionApp.Functions
{
    public class FetchImages
    {
        private readonly BlobStorageService _blobStorageService;

        public FetchImages()
        {
            _blobStorageService = new BlobStorageService();
        }

        [Function("FetchImages")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fetch-images/{jobId}")] HttpRequestData req,
            string jobId,
            FunctionContext context)
        {
            var log = context.GetLogger("FetchImages");

            if (string.IsNullOrEmpty(jobId))
            {
                log.LogError("Job ID is missing.");
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Job ID is required.");
                return badRequestResponse;
            }

            try
            {
                var blobUrls = await _blobStorageService.GetImageUrlsAsync(jobId, log);

                if (!blobUrls.Any())
                {
                    log.LogInformation($"No images found for job ID: {jobId}");
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"No images found for job ID: {jobId}");
                    return notFoundResponse;
                }

                log.LogInformation($"Found {blobUrls.Count} images for job ID: {jobId}");

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteAsJsonAsync(blobUrls);
                return response;
            }
            catch (Exception ex)
            {
                log.LogError($"Error fetching images for job ID: {jobId}. Exception: {ex.Message}");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while fetching images.");
                return errorResponse;
            }
        }
    }
}
