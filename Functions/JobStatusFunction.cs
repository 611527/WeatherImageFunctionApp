using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WeatherImageFunctionApp.Services;

namespace WeatherImageFunctionApp.Functions
{
    public class JobStatusFunction
    {
        private readonly JobStatusService _jobStatusService;

        public JobStatusFunction()
        {
            _jobStatusService = new JobStatusService();
        }

        [Function("GetJobStatus")]
        public async Task<HttpResponseData> GetStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "job-status/{jobId}")] HttpRequestData req,
            string jobId,
            FunctionContext context)
        {
            var log = context.GetLogger("GetJobStatus");

            if (string.IsNullOrEmpty(jobId))
            {
                log.LogError("Job ID is missing.");
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Job ID is required.");
                return badRequestResponse;
            }

            try
            {
                var jobStatus = await _jobStatusService.GetJobStatusAsync(jobId, log);

                if (jobStatus != null)
                {
                    var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(jobStatus);
                    return response;
                }
                else
                {
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync("Job ID not found.");
                    return notFoundResponse;
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error retrieving job status: {ex.Message}");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error retrieving job status.");
                return errorResponse;
            }
        }
    }
}
