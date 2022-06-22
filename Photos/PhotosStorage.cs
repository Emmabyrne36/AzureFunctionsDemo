using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Photos.Models;

namespace Photos
{
    public static class PhotosStorage
    {
        private const string BlobContainerName = "photos";
        private const string CosmosDBName = "photos";
        private const string CollectionName = "metadata";

        [FunctionName("PhotosStorage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob(BlobContainerName, FileAccess.ReadWrite, Connection = Literals.StorageConnectionString)] BlobClient blobClient,
            [CosmosDB(
                CosmosDBName, 
                CollectionName, 
                ConnectionStringSetting = Literals.CosmosDBConnectionString, 
                CreateIfNotExists = true)] IAsyncCollector<dynamic> items,
            ILogger logger)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<PhotoUploadModel>(body);

            var newId = Guid.NewGuid();
            var blobName = $"{newId}.jpg";

            await UploadFile(request, blobName, blobClient);
            await UploadMetadata(items, request, newId);

            logger?.LogInformation($"Successfully uploaded {blobName} file and its metadata");

            return new OkObjectResult(newId);
        }

        private static async Task UploadFile(PhotoUploadModel request, string blobName, BlobClient blobClient)
        {
            // var blobClient = GetBlobClient(blobName);
            var photoBytes = Convert.FromBase64String(request.Photo);

            using (var stream = new MemoryStream(photoBytes))
            {
                await blobClient.UploadAsync(stream);
            }
        }

        private static async Task UploadMetadata(IAsyncCollector<dynamic> items, PhotoUploadModel request, Guid newId)
        {
            var item = new
            {
                id = newId,
                name = request.Name,
                description = request.Description,
                tags = request.Tags
            };

            await items.AddAsync(item);
        }

        // TODO: See if this is needed
        private static BlobClient GetBlobClient(string blobName)
        {
            var connectionString = Environment.GetEnvironmentVariable(Literals.StorageConnectionString);
            var blobClient = new BlobContainerClient(connectionString, BlobContainerName);

            var blob = blobClient.GetBlobClient(blobName);

            return blob;
        }
    }
}
