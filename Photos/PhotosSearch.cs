using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Photos.Models;

namespace Photos
{
    public static class PhotosSearch
    {
        private const string CosmosDBName = "photos";
        private const string CollectionName = "metadata";
        private const string SearchTerm = "sarchTerm";

        [FunctionName("PhotosSearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB(CosmosDBName, CollectionName, ConnectionStringSetting = Literals.CosmosDBConnectionString)] DocumentClient client,
            ILogger logger)
        {
            logger?.LogInformation("Searching...");

            var searchTerm = req.Query[SearchTerm];
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new NotFoundResult();
            }

            var collectionUri = UriFactory.CreateDocumentCollectionUri(CosmosDBName, CollectionName);

            var query = client.CreateDocumentQuery<PhotoUploadModel>(collectionUri, new FeedOptions() { EnableCrossPartitionQuery = true })
             .Where(p => p.Description.Contains(searchTerm))
             .AsDocumentQuery();


            var results = new List<dynamic>();
            while(query.HasMoreResults)
            {
                foreach(var result in await query.ExecuteNextAsync())
                {
                    results.Add(result);
                }
            }

            return new OkObjectResult(results);
        }
    }
}
