using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeatherImageFunctionApp.Models
{
    public class BuienRadarResponse
    {
        public ActualData Actual { get; set; }
    }

    public class ActualData
    {
        public List<StationMeasurement> StationMeasurements { get; set; }
    }

    public class StationMeasurement
    {
        [JsonProperty("stationname")]
        public string StationName { get; set; }

        [JsonProperty("temperature")]
        public string Temperature { get; set; }
    }
}
