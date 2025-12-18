namespace AI_ServiceProvider.Controllers.Services
{
    public interface ITextToSpeechService
    {
        Task<byte[]> SynthesizeSpeechAsync(string text, string voiceName);
        IEnumerable<string> GetAvailableVoices();
    }
}