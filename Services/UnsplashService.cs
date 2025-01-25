using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WeatherImageFunctionApp.Services
{
    public class UnsplashService
    {
        private static readonly string UnsplashApiKey = Environment.GetEnvironmentVariable("UnsplashApiKey");
        private const string UnsplashBaseUrl = "https://api.unsplash.com/photos/random";

        public async Task<string> GetUnsplashImageUrlAsync(ILogger logger)
        {
            try
            {
                if (string.IsNullOrEmpty(UnsplashApiKey))
                {
                    logger.LogError("Unsplash API Key is missing in environment variables.");
                    throw new InvalidOperationException("Unsplash API Key is not set.");
                }

                var unsplashUrl = $"{UnsplashBaseUrl}?client_id={UnsplashApiKey}&query=weather";

                using (var httpClient = new HttpClient())
                {
                    var unsplashResponse = await httpClient.GetStringAsync(unsplashUrl);
                    var unsplashData = JsonConvert.DeserializeObject<dynamic>(unsplashResponse);

                    string imageUrl = unsplashData.urls.regular;
                    logger.LogInformation($"Unsplash Image URL: {imageUrl}");
                    return imageUrl;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error fetching Unsplash image: {ex.Message}");
                throw;
            }
        }
    }
}
