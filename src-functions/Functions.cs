using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace src_functions
{
    public class Functions
    {
        private readonly ILogger<Functions> _logger;

        public Functions(ILogger<Functions> logger)
        {
            _logger = logger;
        }

        [Function("BlobTrigger")]
        public async Task BlobTrigger([BlobTrigger("blobs/{name}", Connection = "connection")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();

            // TODO: Skapa event och skicka till EventGrid
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");

            string endpoint = "https://evgt-dupont.eastus-1.eventgrid.azure.net/api/events";
            string key = "zepu1T7YbODPtxlmtjJPtlFcU+yYBU5pVxA2SS6gugA=";

            EventGridPublisherClient client = new EventGridPublisherClient(new Uri(endpoint), new AzureKeyCredential(key));

            EventGridEvent ev = new EventGridEvent(
                "ExampleEventSubject",
                "Example.EventType",
                "1.0",
                "ExamplePayload"
            );
            client.SendEventAsync(ev).GetAwaiter().GetResult();
            _logger.LogInformation($"Event sent"); // logger
        }
        [Function("EventTrigger")]
        public void EventTrigger([EventGridTrigger] Azure.Messaging.EventGrid.EventGridEvent eventGridEvent)
        {
            // TODO Gör något när EventGrid får ett event
            _logger.LogInformation($"C# EventGrid trigger function processed an event: {eventGridEvent.Data}");
        }
    }
}
