using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WeatherImageFunctionApp.Models;

namespace WeatherImageFunctionApp.Services
{
    public class WeatherService
    {
        private static readonly string BuienRadarUrl = "https://data.buienradar.nl/2.0/feed/json";

        public async Task<List<WeatherStation>> FetchWeatherDataAsync(ILogger logger)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(BuienRadarUrl);
                    var data = JsonConvert.DeserializeObject<BuienRadarResponse>(response);

                    var weatherStations = new List<WeatherStation>();
                    foreach (var station in data.Actual.StationMeasurements)
                    {
                        weatherStations.Add(new WeatherStation
                        {
                            Name = station.StationName,
                            Temperature = station.Temperature
                        });
                    }

                    logger.LogInformation($"Fetched {weatherStations.Count} weather stations from BuienRadar.");
                    return weatherStations;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error fetching weather data: {ex.Message}");
                return new List<WeatherStation>();
            }
        }
    }
}
