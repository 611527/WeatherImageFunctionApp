using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WeatherImageFunctionApp.Helpers;

namespace WeatherImageFunctionApp.Services
{
    public class ImageProcessingService
    {
        public async Task<Stream> GenerateImageAsync(string stationName, string temperature, ILogger logger)
        {
            try
            {
                var stationDisplayName = string.IsNullOrWhiteSpace(stationName) ? "Unknown Station" : stationName;
                var temperatureDisplay = string.IsNullOrWhiteSpace(temperature) ? "No Data" : $"{temperature}°C";

                logger.LogInformation($"Generating image for Station: {stationDisplayName}, Temperature: {temperatureDisplay}");

                var imageGenerator = new ImageGenerator();
                return await imageGenerator.GenerateImageAsync(stationDisplayName, temperatureDisplay, logger);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error generating image: {ex.Message}");
                return null;
            }
        }
    }
}
