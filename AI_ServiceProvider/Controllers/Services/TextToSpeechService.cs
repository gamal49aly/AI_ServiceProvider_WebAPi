using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AI_ServiceProvider.Controllers.Services
{
   
        public class TextToSpeechService : ITextToSpeechService
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly string _apiUrl;

            public TextToSpeechService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                Console.WriteLine("TextToSpeechService constructor has been called!");

                _httpClientFactory = httpClientFactory;
                _apiUrl = configuration["TextToSpeechApiSettings:Url"] ?? "https://your-tts-api-endpoint.com/synthesize";
            }

            public async Task<byte[]> ConvertTextToSpeechAsync(string text, string voiceSettingsJson)
            {
                // --- REPLACE THIS MOCK WITH YOUR AI API CALL ---
                // This is a placeholder to test the flow.
                // Your AI likely expects a JSON payload.
                var requestBody = new { text = text, settings = voiceSettingsJson };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // Create the HttpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
                {
                    Content = jsonContent
                };

                // Send the request
                using var httpClient = _httpClientFactory.CreateClient();
                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Assuming your API returns raw audio bytes in the response body
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"TTS API request failed with status code {response.StatusCode}: {errorContent}");
                }
             
            }
        }

    }

