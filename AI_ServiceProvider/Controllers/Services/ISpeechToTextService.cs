namespace AI_ServiceProvider.Controllers.Services
{
  
        public interface ISpeechToTextService
        {
            Task<string> TranscribeAudioAsync(Stream audioStream, string contentType);
        }
    
}
