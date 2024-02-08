using Azure;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace src_functions
{
    public class Functions
    {
        public class BusMessage
        {
            public string EmailAddress { get; set; }
            public string Subject { get; set; }
            // Add other properties as needed
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

        public Functions(ILogger<Functions> logger )
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


        //[Function("EventTrigger")]
        //public async Task EventTrigger([EventGridTrigger] string eventGridEvent)
        //{
        //    // TODO Gör något när EventGrid får ett event
        //    _logger.LogInformation($"C# EventGrid trigger function processed an event: {eventGridEvent}");
        //}


        [Function("EventTrigger")]
        public async Task EventTrigger([EventGridTrigger] string eventGridEvent)
        {
            // TODO Gör något när EventGrid får ett event
            _logger.LogInformation($"C# EventGrid trigger function processed an event: {eventGridEvent}");


            try
            {
                // Use the raw eventGridEvent as a string
                string messageContent = $"Received Event: {eventGridEvent}";

                // Send the message to Service Bus asynchronously
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

            string connectionString = "Endpoint=sb://sb-dupont.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xx6KMSGW4M1uxU2DwhdY4Y0GijmozD07k+ASbNrzq0c=";
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

                

                //parsing bson

                //string bsonLikeString = Encoding.UTF8.GetString(message.Body);

                
                //BsonDocument bsonDocument = BsonDocument.Parse(bsonLikeString);

                
                //string jsonData = bsonDocument["data"].AsString;

               
                //BsonDocument nestedDocument = BsonDocument.Parse(jsonData);

               
                //string emailAddress = nestedDocument["EmailAddress"].AsString;

                
                //_logger.LogInformation($"C# EmailAddress: {emailAddress}");




               


                
       
                //EventMessage eventMessage = JsonConvert.DeserializeObject<EventMessage>(jsonString);

        
                //string emailAddress = eventMessage.data.EmailAddress;
                //_logger.LogInformation($"C# EmailAddress: {emailAddress}");







                //JObject jsonObject = JObject.Parse(jsonString);

                
                //JObject dataObject = JObject.Parse((string)jsonObject["id"]);

                //_logger.LogInformation($"C# ServiceBus topic trigger function processed message dataObject. {dataObject}");

                //string emailAddress = (string)dataObject["EmailAddress"];

                //_logger.LogInformation($"C# EmailAddress: {emailAddress}");


              





                //string cosmosEndpoint = "https://db-dupont.documents.azure.com:443/";
                //string cosmosKey = "6Z9o0FOJpR4mRZpMIImbq0wDRXBEZzjxHczrQHMHbKzpG6j3R6ZyWzPJWoOmLTJpqjlytj6IqsUcACDbGCSQ7Q==";
                //string databaseName = "dupont-database";
                //string containerName = "dupont-container";

                //CosmosClient cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
                //var database = cosmosClient.GetDatabase(databaseName);
                //var container = database.GetContainer(containerName);




                //var document = new
                //{
                //    id = Guid.NewGuid().ToString(),
                //    Email = email
                //};

                //var result = container.CreateItemAsync(document).Result;




               




                //var document = new
                //{
                //    id = Guid.NewGuid().ToString(),
                //    messageId = message.MessageId,
                //    content = messageContent,
                //    // Add other properties as needed
                //};

                //// Save the document to Cosmos DB
                //await container.CreateItemAsync(document);






             




                // Convert the message body to a string
                //string messageBody = Encoding.UTF8.GetString(message.Body);

                //_logger.LogInformation($"C# Converted to string. {message.Body}");

                ////// Extract email address from the message
                //string emailAddress = ExtractEmailAddress(messageBody);

                //if (!string.IsNullOrEmpty(emailAddress))
                //{
                //    _logger.LogInformation($"Extracted email address: {emailAddress}");

                //    // Save email address to Cosmos DB or perform other actions
                //    //SaveEmailAddressToCosmosDB(emailAddress);
                //}
                //else
                //{
                //    _logger.LogWarning("Email address not found in the message.");
                //}



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Service Bus message.");
            }
        }







        private string ExtractEmailAddress(string messageBody)
        {
            try
                {

                 JObject jsonObject = JObject.Parse(messageBody);

        
            
                string emailAddress = (string)jsonObject["data"];
                return emailAddress;
        
       
                }
            catch (JsonReaderException ex)
            {
        
                _logger.LogError(ex, "Error parsing JSON message body.");
                return null; 
            }
        }





        //    [Function("ServiceBusTrigger")]
        //    public void ServiceBusTrigger([ServiceBusTrigger(

        //"sb-users",
        //"sbs-dupont",
        //Connection = "sbConnectionString")]string message)
        //    {
        //        try
        //        {
        //            _logger.LogInformation($"C# ServiceBus topic trigger function processed message: {message}");

        //            // Extract email address from the message
        //            string emailAddress = ExtractEmailAddress(message);

        //            if (!string.IsNullOrEmpty(emailAddress))
        //            {
        //                _logger.LogInformation($"Extracted email address: {emailAddress}");

        //                //// Save email address to Cosmos DB
        //                //SaveEmailAddressToCosmosDB(emailAddress);
        //            }
        //            else
        //            {
        //                _logger.LogWarning("Email address not found in the message.");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error processing Service Bus message.");
        //        }



        //    }

        //    private string ExtractEmailAddress(string message)
        //    {
        //        

        //        JObject innerObject = JObject.Parse(message);

        //        
        //        string emailAddress = (string)innerObject["data"];

        //        //JObject outerObject = JObject.Parse(dataJsonString);

        //        
        //        //string emailAddress = (string)outerObject["EmailAddress"];





        //        //string messageBody = Encoding.UTF8.GetString(message.Body);
        //        
        //        //JObject jsonObject = JObject.Parse(messageBody);
   
        //        //string emailAddress = (string)jsonObject["EmailAddress"];

        //        return emailAddress;
        //    }

        


     

        private static string ExtractEmailFromMessage(string message)
        {

            try
            {
                var jsonObject = JsonConvert.DeserializeObject<BusMessage>(message);
                return jsonObject.EmailAddress;
            }
            catch (Newtonsoft.Json.JsonException)
            {

                return null;
            }

        }

        private static void SaveEmailToCosmosDB(string email, ILogger log)
        {
            string cosmosEndpoint = "https://db-dupont.documents.azure.com:443/";
            string cosmosKey = "6Z9o0FOJpR4mRZpMIImbq0wDRXBEZzjxHczrQHMHbKzpG6j3R6ZyWzPJWoOmLTJpqjlytj6IqsUcACDbGCSQ7Q==";
            string databaseName = "dupont-database";
            string containerName = "dupont-container";

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

    }
}



