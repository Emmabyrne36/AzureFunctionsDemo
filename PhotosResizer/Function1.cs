using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace PhotosResizer
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([BlobTrigger("photos/{name}", Connection = "")]Stream myBlob, 
            [Blob("photos-small/{name}", FileAccess.Write, Connection = Literals.StrageConnectionString)]
            ILogger logger)
        {
            logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
