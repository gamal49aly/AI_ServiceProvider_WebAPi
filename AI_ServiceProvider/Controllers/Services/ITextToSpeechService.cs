namespace AI_ServiceProvider.Controllers.Services
{
    public interface ITextToSpeechService
    {
        Task<byte[]> ConvertTextToSpeechAsync(string text, string voiceSettingsJson);
    }
}
