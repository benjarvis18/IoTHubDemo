using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts
{
    public class HeaterStateChange
    {
        [JsonProperty("isHeaterOn")]
        public bool IsHeaterOn { get; set; }

        [JsonProperty("targetTemperatureMinimum")]
        public double TargetTemperatureMinimum { get; set; }

        [JsonProperty("targetTemperatureMaximum")]
        public double TargetTemperatureMaximum { get; set; }
    }
}
