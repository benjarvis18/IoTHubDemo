using Newtonsoft.Json;
using System;

namespace Contracts
{
    public class TemperatureReading
    {
        [JsonProperty("temperature")]
        public decimal Temperature { get; set; }
    }
}
