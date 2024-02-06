using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using src_functions.Models;
using System.Text.Json;

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

            string endpoint = Environment.GetEnvironmentVariable("TopicEndpoint") ?? "";
            string key = Environment.GetEnvironmentVariable("TopicKey") ?? "";

            EventGridPublisherClient client = new EventGridPublisherClient(new Uri(endpoint), new AzureKeyCredential(key));

            EventGridEvent ev = new EventGridEvent(
                "ExampleEventSubject",
                "Example.EventType",
                "1.0",
                content
            );
            client.SendEventAsync(ev).GetAwaiter().GetResult();
            _logger.LogInformation($"Event sent"); // logger
        }
        [Function("EventTrigger")]
        public async Task EventTrigger([EventGridTrigger] string eventGridEvent)
        {
            // TODO Gör något när EventGrid får ett event
            _logger.LogInformation($"C# EventGrid trigger function processed an event: {eventGridEvent}");
        }

        public async Task<DrinkModel> GetDrinkFromApi()
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync("https://www.thecocktaildb.com/api/json/v1/1/random.php");

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                DrinkApiModel? drinkApiModel = JsonSerializer.Deserialize<DrinkApiModel>(responseContent);

                if (drinkApiModel != null && drinkApiModel.drinks != null && drinkApiModel.drinks[0] != null)
                {
                    DrinkModel drinkModel = new();

                    drinkModel.Name = drinkApiModel.drinks[0].strDrink;

                    foreach (var property in drinkApiModel.drinks[0].GetType().GetProperties())
                    {
                        if (property.Name.StartsWith("strIngredient"))
                        {
                            var propertyValue = property.GetValue(drinkApiModel.drinks[0]);

                            if (propertyValue != null)
                            {
                                string? ingredient = propertyValue.ToString();

                                if (!string.IsNullOrWhiteSpace(ingredient))
                                {
                                    drinkModel.Ingredients.Add(ingredient);
                                }
                            }
                        }
                    }

                    return drinkModel;
                }
            }

            throw new Exception("Error! Could not retrieve data from api.");
        }
    }
}
