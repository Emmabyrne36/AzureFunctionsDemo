using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Photos.Enums;

namespace Photos.Functions
{
    public static class PhotosDownload
    {
        // TODO: Check this interpolation works like this
        private const string BlobPathSmall = "photos-small/{id}.jpg";
        private const string BlobPathMedium = "photos-medium/{id}.jpg";
        private const string PhotoRoute = "photos/{id}";

        private const string Small = "sm";
        private const string Medium = "md";
        private const string Original = "original";

        [FunctionName(FunctionNames.PhotosDownload)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = PhotoRoute)] HttpRequest req,
            [Blob(BlobPathSmall, FileAccess.Read, Connection = Literals.StorageConnectionString)] Stream imageSmall,
            [Blob(BlobPathMedium, FileAccess.Read, Connection = Literals.StorageConnectionString)] Stream imageMedium,
            [Blob(PhotoRoute, FileAccess.Read, Connection = Literals.StorageConnectionString)] Stream imageOriginal,
            Guid id,
            ILogger logger)
        {
            logger?.LogInformation($"Downloading {id}...");
            
            byte[] data = (string)req.Query["size"] switch
            {
                Small => await GetBytesFromStreamAsync(imageSmall, nameof(Small), logger),
                Medium => await GetBytesFromStreamAsync(imageMedium, nameof(Medium), logger),
                _ => await GetBytesFromStreamAsync(imageOriginal, Original, logger),
            };

            return new FileContentResult(data, "image/jpeg")
            {
                FileDownloadName = $"{id}.jpg"
            };
        }

        private static async Task<byte[]> GetBytesFromStreamAsync(Stream stream, string size, ILogger logger)
        {
            logger?.LogInformation($"Retrieving the {size.ToLower()} size");

            var data = new byte[stream.Length];
            await stream.ReadAsync(data, 0, data.Length);
            return data;
        }


    }
}
