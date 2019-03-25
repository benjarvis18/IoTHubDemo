using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Contracts;
using Microsoft.Azure.Devices;
using System;
using System.Threading.Tasks;

namespace Functions
{
    public static class ReceiveTemperatureReading
    {
        private static readonly decimal _targetTemperatureMinimum = 20;
        private static readonly decimal _targetTemperatureMaximum = 25;

        private static readonly ServiceClient _serviceClient = ServiceClient.CreateFromConnectionString(Environment.GetEnvironmentVariable("IoTHubCloudToDeviceConnectionString"));

        private static async Task ChangeHeaterStateAsync(string deviceId, bool isHeaterOn)
        {
            var methodInvocation = new CloudToDeviceMethod("ChangeHeaterState") { ResponseTimeout = TimeSpan.FromSeconds(30) };
            methodInvocation.SetPayloadJson(JsonConvert.SerializeObject(new HeaterStateChange() { IsHeaterOn = isHeaterOn }));

            await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
        }

        [FunctionName("ReceiveTemperatureReading")]
        public static async Task Run([IoTHubTrigger("messages/events", Connection = "IoTHubReceiveMessageConnectionString")]EventData message, ILogger log)
        {
            // Parse the message
            var reading = JsonConvert.DeserializeObject<TemperatureReading>(Encoding.UTF8.GetString(message.Body.Array));
            var deviceId = message.SystemProperties["iothub-connection-device-id"].ToString();

            bool.TryParse(message.Properties["isHeaterOn"].ToString(), out bool isHeaterOn);

            // If the temperature is below the minimum then switch the heater on
            if (reading.Temperature < _targetTemperatureMinimum)
            {
                log.LogInformation("Temperature is lower than the minimum. Turning heater on.");

                // Switch heater on
                if (!isHeaterOn)
                {
                    await ChangeHeaterStateAsync(deviceId, true);
                }
            }
            // If the temperature is above the maximum then switch the heater off
            else if (reading.Temperature > _targetTemperatureMaximum)
            {
                log.LogInformation("Temperature is higher than the maximum. Turning heater off.");

                // Switch heater off
                if (isHeaterOn)
                {
                    await ChangeHeaterStateAsync(deviceId, false);
                }
            }
        }
    }
}