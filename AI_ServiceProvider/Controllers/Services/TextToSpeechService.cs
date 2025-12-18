using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AI_ServiceProvider.Controllers.Services
{
    public class TextToSpeechService : ITextToSpeechService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TextToSpeechService> _logger;
        private readonly string _apiUrl;

        private static readonly HashSet<string> ValidVoices = new(StringComparer.OrdinalIgnoreCase)
        {
            "Zephyr", "Puck", "Charon", "Kore", "Fenrir", "Leda", "Orus",
            "Aoede", "Callirrhoe", "Autonoe", "Enceladus", "Iapetus",
            "Umbriel", "Algieba", "Despina", "Erinome", "Algenib",
            "Rasalgethi", "Laomedeia", "Achernar", "Alnilam", "Schedar",
            "Gacrux", "Pulcherrima", "Achird", "Zubenelgenubi",
            "Vindemiatrix", "Sadachbia", "Sadaltager", "Sulafat"
        };

        public TextToSpeechService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<TextToSpeechService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiUrl = configuration["TextToSpeechApiSettings:Url"]
                ?? throw new ArgumentNullException("TextToSpeechApiSettings:Url is not configured");
        }

        public async Task<byte[]> SynthesizeSpeechAsync(string text, string voiceName)
        {
            try
            {
                if (!ValidVoices.Contains(voiceName))
                {
                    _logger.LogWarning("Invalid voice '{Voice}' requested, using default 'Kore'", voiceName);
                    voiceName = "Kore";
                }

                _logger.LogInformation("Synthesizing speech with voice '{Voice}' for text length {Length}",
                    voiceName, text.Length);

                var requestBody = new { text, voice = voiceName };
                var jsonString = JsonSerializer.Serialize(requestBody);

                _logger.LogInformation("Sending to URL: {Url}", _apiUrl);

                var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                // ✅ Create client with timeout
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMinutes(2); // 2 minute timeout

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                _logger.LogInformation("Sending POST request...");
                var response = await httpClient.PostAsync(_apiUrl, jsonContent);

                stopwatch.Stop();
                _logger.LogInformation("Response received in {ElapsedMs}ms with status {StatusCode}",
                    stopwatch.ElapsedMilliseconds, response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var audioBytes = await response.Content.ReadAsByteArrayAsync();
                    _logger.LogInformation("Successfully synthesized {Size} bytes of audio", audioBytes.Length);

                    if (audioBytes.Length == 0)
                    {
                        throw new HttpRequestException("Received empty audio data from TTS API");
                    }

                    return audioBytes;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("TTS API failed with status {Status}: {Error}",
                    response.StatusCode, errorContent);

                throw new HttpRequestException(
                    $"TTS API request failed with status code {response.StatusCode}: {errorContent}"
                );
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "TTS API request timed out");
                throw new HttpRequestException("TTS API request timed out. The service may be unreachable.", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during speech synthesis");
                throw;
            }
        }

        public IEnumerable<string> GetAvailableVoices()
        {
            return ValidVoices.OrderBy(v => v);
        }
    }
}