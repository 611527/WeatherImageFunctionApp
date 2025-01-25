using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using WeatherImageFunctionApp.Models;

namespace WeatherImageFunctionApp.Helpers
{
    public static class WeatherFetcher
    {
        private const string BuienRadarUrl = "https://data.buienradar.nl/2.0/feed/json";

        public static List<WeatherStation> FetchWeatherData()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetStringAsync(BuienRadarUrl).Result;
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

                return weatherStations;
            }
        }
    }
}
