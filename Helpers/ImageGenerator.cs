using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WeatherImageFunctionApp.Services;

namespace WeatherImageFunctionApp.Helpers
{
    public class ImageGenerator
    {
        private readonly UnsplashService _unsplashService;
        private readonly TextOverlayService _textOverlayService;

        public ImageGenerator()
        {
            _unsplashService = new UnsplashService();
            _textOverlayService = new TextOverlayService();
        }

        public async Task<Stream> GenerateImageAsync(string stationName, string temperature, ILogger logger)
        {
            try
            {
                var stationDisplayName = string.IsNullOrWhiteSpace(stationName) ? "Unknown Station" : stationName;
                var temperatureDisplay = string.IsNullOrWhiteSpace(temperature) ? "No Data" : $"{temperature}°C";

                logger.LogInformation($"Generating image for Station: {stationDisplayName}, Temperature: {temperatureDisplay}");

                var imageUrl = await _unsplashService.GetUnsplashImageUrlAsync(logger);

                using (var httpClient = new HttpClient())
                {
                    using (var imageStream = await httpClient.GetStreamAsync(imageUrl))
                    {
                        using (var image = Image.FromStream(imageStream))
                        using (var bitmap = new Bitmap(image))
                        {
                            return _textOverlayService.ApplyTextOverlay(bitmap, $"{stationDisplayName}: {temperatureDisplay}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in GenerateImageAsync: {ex.Message}");
                throw;
            }
        }
    }
}
