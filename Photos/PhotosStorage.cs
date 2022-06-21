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

        [FunctionName("PhotosStorage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<PhotoUploadModel>(body);

            var newId = Guid.NewGuid();
            var blobName = $"{newId}.jpg";

            var blob = GetBlobClient(blobName);
            var photoBytes = Convert.FromBase64String(request.Photo);

            using (var stream = new MemoryStream(photoBytes))
            {
                await blob.UploadAsync(stream);
            }

            logger?.LogInformation($"Successfully uploaded {blobName} file");

            return new OkObjectResult(newId);
        }

        private static BlobClient GetBlobClient(string blobName)
        {
            var connectionString = Environment.GetEnvironmentVariable(Literals.StorageConnectionString);
            var blobClient = new BlobContainerClient(connectionString, BlobContainerName);

            var blob = blobClient.GetBlobClient(blobName);

            return blob;
        }
    }
}
