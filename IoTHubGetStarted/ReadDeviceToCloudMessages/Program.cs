using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace ReadDeviceToCloudMessages
{
    public class Program
    {
        static string connectionString = "Endpoint=sb://ihsuprodblres074dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=QQv/MMvl8PaU7Lt9reZmnXTRS6QXWmK74uvX0kXygo4=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        public static void Main(string[] args)
        {
            Console.WriteLine("Receive messages. Ctrl-C to exit.\n");
            var builder = new EventHubsConnectionStringBuilder(connectionString)
                {
                    EntityPath = "iothub-ehub-raspberrys-327922-21122db859"
                };

            eventHubClient = EventHubClient.CreateFromConnectionString(builder.ToString());
            
            var d2cPartitions = eventHubClient.GetRuntimeInformationAsync().Result;

            CancellationTokenSource cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions.PartitionIds)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.CreateReceiver(iotHubD2cEndpoint, partition, DateTime.UtcNow);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                var eventData = await eventHubReceiver.ReceiveAsync(1);
                if (eventData == null) continue;

                string data = Encoding.UTF8.GetString(eventData.Single().Body.Array);
                Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);
            }
        }
    }
}
