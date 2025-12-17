
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AI_ServiceProvider.Controllers.Services
{
    public class SpeechToTextService : ISpeechToTextService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public SpeechToTextService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            Console.WriteLine("SpeechToTextService constructor has been called!");

            _httpClientFactory = httpClientFactory;
            _apiUrl = configuration["SpeechToTextApiSettings:Url"] ?? "https://your-stt-api-endpoint.com/transcribe";
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream, string contentType)
        {
            // Create the audio content and explicitly set its Content-Type header
            var audioContent = new StreamContent(audioStream);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // Create the form-data content
            using var content = new MultipartFormDataContent();

            // Add the audio stream to the content
            // The API expects a file part named "audio"
            content.Add(audioContent, "audio", "upload.wav");

            // Create the HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
            {
                Content = content
            };

            // Send the request
            using var httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Assuming the API returns the transcribed text as a plain string
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"STT API request failed with status code {response.StatusCode}: {errorContent}");
            }
        }
    }

}
