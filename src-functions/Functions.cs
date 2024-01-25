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
        }
        [Function("EventTrigger")]
        public void EventTrigger([EventGridTrigger] Azure.Messaging.EventGrid.EventGridEvent eventGridEvent)
        {
            // TODO Gör något när EventGrid får ett event
            _logger.LogInformation($"C# EventGrid trigger function processed an event: {eventGridEvent.Data}");
        }
    }
}
