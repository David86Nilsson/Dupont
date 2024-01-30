using Azure.Storage.Blobs;
using src_frontend.Models;
using System.Text.Json;


namespace src_frontend.Services
{
    public class DataService
    {
        BlobServiceClient blobServiceClient;
        BlobContainerClient containerClient;

        public DataService()
        {
            blobServiceClient = new BlobServiceClient("storageAccount");
            containerClient = blobServiceClient.GetBlobContainerClient("blobs");
        }

        public bool AddToStorage(string emailAddress)
        {
            string id = Guid.NewGuid().ToString();

            Subscriber subscriber = new()
            {
                Id = id,
                EmailAddress = emailAddress
            };

            string json = JsonSerializer.Serialize(subscriber);
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            string blobName = Guid.NewGuid().ToString() + ".json";

            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                if (containerClient.UploadBlob(blobName, memoryStream).GetRawResponse().IsError)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
