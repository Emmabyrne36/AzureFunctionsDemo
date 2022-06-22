namespace Photos.AnalyserService.Abstractions
{
    public interface IAnalyserService
    {
        Task<dynamic> AnalyseAsync(byte[] image);
    }
}
