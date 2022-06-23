using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Photos.AnalyserService.Abstractions;
using Photos.Enums;

namespace Photos.Functions
{
    public class PhotosAnalyser
    {
        private readonly IAnalyserService _analyserService;

        public PhotosAnalyser(IAnalyserService analyserService)
        {
            _analyserService = analyserService ?? throw new ArgumentNullException(nameof(analyserService));
        }

        [FunctionName(FunctionNames.PhotosAnalyser)]
        public async Task<dynamic> Run([ActivityTrigger] List<byte> image)
        {
            return await _analyserService.AnalyseAsync(image.ToArray());
        }
    }
}
