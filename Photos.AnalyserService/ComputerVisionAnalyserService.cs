using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using Photos.AnalyserService.Abstractions;

namespace Photos.AnalyserService
{
    public class ComputerVisionAnalyserService : IAnalyserService
    {
        // The values for these in local.settings.json are fake values and should be replaced by the values from the Cognitive service in Azure
        private const string VisKey = "VisionKey";
        private const string VisEndpont = "VisionEndpoint";

        private readonly ComputerVisionClient _client;

        // This service is set up to connect to a Computer Vision cognitive service in Azure
        public ComputerVisionAnalyserService(IConfiguration configuration)
        {
            var visionKey = configuration[VisKey];
            var visionEndpoint = configuration[VisEndpont];

            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionKey))
            {
                Endpoint = visionEndpoint
            };
        }

        public async Task<dynamic> AnalyseAsync(byte[] image)
        {
            using (var ms = new MemoryStream(image))
            {
                var imageAnalysis = await _client.AnalyzeImageInStreamAsync(ms);

                var result = new
                {
                    Metadata = new
                    {
                        Width = imageAnalysis.Metadata.Width,
                        Height = imageAnalysis.Metadata.Height,
                        Format = imageAnalysis.Metadata.Format,
                    },
                    Categories = imageAnalysis.Categories.Select(c => c.Name).ToArray()
                };

                return result;
            }
        }
    }
}
