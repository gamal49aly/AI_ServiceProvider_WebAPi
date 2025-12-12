namespace AI_ServiceProvider.Controllers.Services
{

    // Placeholder interface  AI service
    public interface IImageParsingService
    {
        Task<string> ParseImageAsync(Stream imageStream, string jsonKeys);
    }

}
