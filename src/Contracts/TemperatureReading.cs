using Newtonsoft.Json;
using System;

namespace Contracts
{
    public class TemperatureReading
    {
        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("targetTemperatureMinimum")]
        public double TargetTemperatureMinimum { get; set; }

        [JsonProperty("targetTemperatureMaximum")]
        public double TargetTemperatureMaximum { get; set; }
    }
}
