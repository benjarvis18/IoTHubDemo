using Contracts;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Device
{
    class Program
    {
        private static DeviceClient _deviceClient;
        private static bool _isHeaterOn = false;

        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private static async Task<MethodResponse> ChangeHeaterState(MethodRequest methodRequest, object userContext)
        {            
            var heaterStateChange = JsonConvert.DeserializeObject<HeaterStateChange>(Encoding.UTF8.GetString(methodRequest.Data));

            await _semaphore.WaitAsync();

            if (_isHeaterOn != heaterStateChange.IsHeaterOn)
            {
                _isHeaterOn = heaterStateChange.IsHeaterOn;

                var heaterState = heaterStateChange.IsHeaterOn ? "On" : "Off";
                Console.WriteLine($"{DateTime.UtcNow} - Received Message - Change Heater State to {heaterState}");
            }

            _semaphore.Release();

            // Acknowlege the direct method call with a 200 success message
            return new MethodResponse(200);
        }

        private static async Task SimulateDeviceAsync()
        {
            var currentTemperature = 25;
            var rand = new Random();

            while (true)
            {
                // Get a new temperature between 1 and 5 degrees
                var temperatureDifference = rand.Next(1, 5);

                // If the heater is on we add the value, otherwise we subtract
                if (!_isHeaterOn)
                {
                    temperatureDifference *= -1;
                }

                currentTemperature += temperatureDifference;

                // Create the message and upload to IoT Hub
                var reading = new TemperatureReading() { Temperature = currentTemperature };

                string messageString = JsonConvert.SerializeObject(reading);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("isHeaterOn", _isHeaterOn.ToString());

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.UtcNow} - Sent Message - {messageString}");

                // Wait 1 second before sending the next message
                await Task.Delay(1000);
            }
        }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            Console.WriteLine("Starting Simulated Application");

            _deviceClient = DeviceClient.CreateFromConnectionString(configuration.GetConnectionString("IoTHubConnectionString"), TransportType.Mqtt);
            _deviceClient.SetMethodHandlerAsync("ChangeHeaterState", ChangeHeaterState, null).Wait();

            SimulateDeviceAsync().Wait();
        }
    }
}
