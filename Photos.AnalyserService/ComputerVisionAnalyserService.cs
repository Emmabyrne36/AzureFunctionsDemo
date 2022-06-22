using Photos.AnalyserService.Abstractions;

namespace Photos.AnalyserService
{
    public class ComputerVisionAnalyserService : IAnalyserService
    {
        public Task<dynamic> AnalyseAsync(byte[] image)
        {
            throw new NotImplementedException();
        }
    }
}
