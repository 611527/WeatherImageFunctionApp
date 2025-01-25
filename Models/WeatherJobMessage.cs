using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherImageFunctionApp.Models
{
    public class WeatherJobMessage
    {
        public string JobId { get; set; }
        public string StationName { get; set; }
        public string Temperature { get; set; }
    }
}
