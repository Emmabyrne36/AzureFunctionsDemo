using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Photos.Enums;
using Photos.Models;

namespace Photos.Functions
{
    public class PhotosStorage
    {
        private const string BlobContainerName = "photos";
        private const string CosmosDBName = "photos";
        private const string CollectionName = "metadata";

        [FunctionName(FunctionNames.PhotosStorage)]
        public async Task<byte[]> Run(
            [ActivityTrigger] PhotoUploadModel request,
            [Blob(BlobContainerName, FileAccess.ReadWrite, Connection = Literals.StorageConnectionString)] BlobClient blobClient,
            [CosmosDB(
                CosmosDBName, 
                CollectionName, 
                ConnectionStringSetting = Literals.CosmosDBConnectionString, 
                CreateIfNotExists = true)] IAsyncCollector<dynamic> items,
            ILogger logger)
        {

            var newId = Guid.NewGuid();
            var blobName = $"{newId}.jpg";

            var photoBytes = await UploadFile(request, blobName, blobClient);

            await UploadMetadata(items, request, newId);

            logger?.LogInformation($"Successfully uploaded {blobName} file and its metadata");

            return photoBytes;
        }

        private async Task<byte[]> UploadFile(PhotoUploadModel request, string blobName, BlobClient blobClient)
        {
            // var blobClient = GetBlobClient(blobName);
            var photoBytes = Convert.FromBase64String(request.Photo);

            using (var stream = new MemoryStream(photoBytes))
            {
                await blobClient.UploadAsync(stream);
            }

            return photoBytes;
        }

        private static async Task UploadMetadata(IAsyncCollector<dynamic> items, PhotoUploadModel request, Guid newId)
        {
            var item = new
            {
                Id = newId,
                Name = request.Name,
                Description = request.Description,
                Tags = request.Tags,
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
