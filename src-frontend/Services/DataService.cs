using Azure.Storage.Blobs;
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

        public bool AddToStorage(string data)
        {
            string json = JsonSerializer.Serialize(data);
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);

            string blobName = "blob_" + Guid.NewGuid().ToString() + ".json";

            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                if (containerClient.UploadBlob(blobName, memoryStream).Value != null)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
