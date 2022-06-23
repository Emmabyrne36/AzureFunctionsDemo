using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Photos.Enums;
using Photos.Models;

namespace Photos.Functions
{
    public static class PhotosOrchestrator
    {

        [FunctionName("PhotosOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var body = await req.Content.ReadAsStringAsync();

            var request = JsonConvert.DeserializeObject<PhotoUploadModel>(body);

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(FunctionNames.PhotosOrchestrator, request);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(FunctionNames.PhotosOrchestrator)]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var model = context.GetInput<PhotoUploadModel>();
            var photoBytes = await context.CallActivityAsync<byte[]>(FunctionNames.PhotosStorage, model);
            var analysis = await context.CallActivityAsync<dynamic>(FunctionNames.PhotosAnalyser, photoBytes.ToList());

            return analysis;
        }
    }
}