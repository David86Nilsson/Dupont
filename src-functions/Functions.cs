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
        public async Task Run([BlobTrigger("blobs/{name}", Connection = "connection")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }
    }
}
