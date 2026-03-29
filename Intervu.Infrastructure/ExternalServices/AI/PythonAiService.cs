using Intervu.Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    public class PythonAiService : IPythonAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PythonAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["PythonAiService:BaseUrl"] ?? "http://localhost:8000";
        }

        public async Task<string> ExtractDocumentToJsonAsync(Stream fileStream, string fileName, string docType)
        {
            var url = $"{_baseUrl}/api/extract-document";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            using var content = new MultipartFormDataContent();
            
            // File part
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(streamContent, "file", fileName);
            
            // doc_type string part
            content.Add(new StringContent(docType), "doc_type");

            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
