using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace WeatherImageFunctionApp.Services
{
    public class BlobStorageService
    {
        private static readonly string BlobConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private const string BlobContainerName = "weather-images";

        public async Task<List<string>> GetImageUrlsAsync(string jobId, ILogger logger)
        {
            try
            {
                if (string.IsNullOrEmpty(BlobConnectionString))
                {
                    logger.LogError("AzureWebJobsStorage connection string is not configured.");
                    throw new InvalidOperationException("Storage connection string is not set.");
                }

                var blobServiceClient = new BlobServiceClient(BlobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobUrls = new List<string>();

                var blobs = containerClient.GetBlobs(prefix: $"{jobId}/").ToList();

                foreach (var blobItem in blobs)
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    blobUrls.Add(blobClient.Uri.ToString());
                }

                return blobUrls;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error retrieving images from Blob Storage: {ex.Message}");
                throw;
            }
        }
        public async Task UploadImageAsync(string blobName, Stream imageStream, ILogger logger)
        {
            try
            {
                if (string.IsNullOrEmpty(BlobConnectionString))
                {
                    logger.LogError("AzureWebJobsStorage connection string is not configured.");
                    throw new InvalidOperationException("Storage connection string is not set.");
                }

                var blobServiceClient = new BlobServiceClient(BlobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.UploadAsync(imageStream, overwrite: true);
                logger.LogInformation($"Image uploaded to blob storage: {blobName}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error uploading image to Blob Storage: {ex.Message}");
                throw;
            }
        }
    }
}
