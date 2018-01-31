using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Extensions.Configuration;

namespace CreateDeviceIdentity
{
    public class Program
    {
        static RegistryManager registryManager;
        
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();

            builder.AddUserSecrets("User-Secret-ID");

            var configurationRoot = builder.Build();

            var connectionString = $"HostName=raspberrystreetdemo.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey={configurationRoot["iothub-owner-sas"]}";

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync().Wait();
            Console.ReadLine();
        }

        private static async Task AddDeviceAsync()
        {
            string deviceId = "dot-net-core";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
