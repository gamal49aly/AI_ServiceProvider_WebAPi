using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

namespace AI_ServiceProvider.Controllers.Services
{
    public class ImageParsingService : IImageParsingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public ImageParsingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            Console.WriteLine("ImageParsingService constructor has been called!");

            _httpClientFactory = httpClientFactory;
            _apiUrl = configuration["VisionApiSettings:Url"] ?? "https://qwen-vision-api-196607509856.europe-west1.run.app/extract";
        }


        public async Task<string> ParseImageAsync(Stream imageStream, string contentType, string jsonKeys)
        {
            // Create an HttpClient instance
            using var httpClient = _httpClientFactory.CreateClient();

            // Create the image content and explicitly set its Content-Type header
            var imageContent = new StreamContent(imageStream);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            //  Create the form-data content
            using var content = new MultipartFormDataContent();

            // Add the image stream to the content
            // The API expects a file part named "image"
            content.Add(imageContent, "image", "upload.jpg");

            // Add the JSON keys to the content
            // The API expects a form field named "json_keys"
            content.Add(new StringContent(jsonKeys, Encoding.UTF8, "application/json"), "json_keys");

            //  Create the HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
            {
                Content = content
            };

            //  The request
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
             
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status code {response.StatusCode}: {errorContent}");
            }


        }
    }
}
