using Azure;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using src_functions.Models;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace src_functions
{
    public class Functions
    {
        public class BusMessage
        {
            public string EmailAddress { get; set; }
            public string Subject { get; set; }
        }

        public class EventData
        {
            public string Id { get; set; }
            public string EmailAddress { get; set; }
        }

        public class EventMessage
        {
            public string id { get; set; }
            public string Source { get; set; }
            public EventData data { get; set; }
        }

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
            _logger.LogInformation($"Event sent");
        }

        [Function("EventTrigger")]
        public async Task EventTrigger([EventGridTrigger] string eventGridEvent)
        {
            _logger.LogInformation($"C# EventGrid trigger function processed an event: {eventGridEvent}");

            try
            {
                string messageContent = eventGridEvent;

                await SendMessageToServiceBusAsync(messageContent);

                _logger.LogInformation($"Message sent to Service Bus: {messageContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing event: {ex.Message}");
            }
        }

        private async Task SendMessageToServiceBusAsync(string messageContent)
        {

            string connectionString = Environment.GetEnvironmentVariable("sbConnectionString");
            string topicName = "sb-users";

            var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(topicName);

            try
            {
                var message = new ServiceBusMessage(messageContent);

                await sender.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message to Service Bus: {ex.Message}");
            }
            finally
            {
                await sender.CloseAsync();
            }
        }

        [Function("ServiceBusTrigger")]
        public async Task ServiceBusTrigger([ServiceBusTrigger(

        "sb-users",
        "sbs-dupont",
        Connection = "sbConnectionString")] ServiceBusReceivedMessage message)
        {
            try
            {
                _logger.LogInformation($"C# ServiceBus topic trigger function processed message. {message.Body}");

                ServiceBusDataModel? serviceBusDataModel = System.Text.Json.JsonSerializer.Deserialize<ServiceBusDataModel>(message.Body);
                if (serviceBusDataModel == null)
                {
                    throw new Exception("ServiceBusDataModel is null");
                }
                else if (serviceBusDataModel.data == null)
                {
                    throw new Exception("ServiceBusDataModel.data is null");
                }
                EmailDataModel? emailDataModel = System.Text.Json.JsonSerializer.Deserialize<EmailDataModel>(serviceBusDataModel.data);
                if (serviceBusDataModel != null)
                {
                    emailDataModel = System.Text.Json.JsonSerializer.Deserialize<EmailDataModel>(serviceBusDataModel.data);
                }
                else
                {
                    throw new Exception("EmailDataModel is null");
                }

                _logger.LogInformation($"C# ServiceBus topic trigger extracted EmailAddress - {emailDataModel.EmailAddress}");

                SaveEmailToCosmosDB(emailDataModel.EmailAddress, _logger);

                await TriggerLogicApp(await GetEmailModel(emailDataModel.EmailAddress));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public void SaveEmailToCosmosDB(string email, ILogger log)
        {
            string? cosmosEndpoint = Environment.GetEnvironmentVariable("CosmosEndpoint");
            string? cosmosKey = Environment.GetEnvironmentVariable("CosmosPrimaryKey");
            string? databaseName = Environment.GetEnvironmentVariable("CosmosDatabaseName");
            string? containerName = Environment.GetEnvironmentVariable("CosmosContainerName");

            CosmosClient cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
            var database = cosmosClient.GetDatabase(databaseName);
            var container = database.GetContainer(containerName);

            var document = new
            {
                id = Guid.NewGuid().ToString(),
                Email = email
            };

            var result = container.CreateItemAsync(document).Result;
            log.LogInformation($"Email saved to Cosmos DB. Request charge: {result.RequestCharge}");
        }

        public async Task<EmailModel> GetEmailModel(string emailAddress)
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(Environment.GetEnvironmentVariable("CocktailDbRequestUri"));
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                DrinkApiModel? drinkApiModel = System.Text.Json.JsonSerializer.Deserialize<DrinkApiModel>(responseContent);

                if (drinkApiModel != null && drinkApiModel.drinks != null && drinkApiModel.drinks[0] != null)
                {
                    EmailModel emailModel = new();

                    emailModel.EmailAddress = emailAddress;

                    emailModel.DrinkName = drinkApiModel.drinks[0].strDrink;

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
                                    emailModel.DrinkIngredients.Add(ingredient);
                                }
                            }
                        }
                    }

                    return emailModel;
                }
            }

            throw new Exception("Error! Could not retrieve data from api.");
        }

        public async Task TriggerLogicApp(EmailModel emailModel)
        {
            HttpClient httpClient = new();

            HttpContent httpContent = new StringContent(JsonSerializer.Serialize(emailModel), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(Environment.GetEnvironmentVariable("LogicAppEndPoint"), httpContent);
        }
    }
}
