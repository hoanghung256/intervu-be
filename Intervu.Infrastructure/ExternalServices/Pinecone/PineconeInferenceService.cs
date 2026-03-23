using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;

namespace Intervu.Infrastructure.ExternalServices.Pinecone
{
    public class PineconeInferenceService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiVersion;
        private readonly string _model;
        private readonly int? _dimension;

        public PineconeInferenceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["PineCone:PINECONE_API_KEY"] ?? throw new ArgumentNullException("Pinecone API Key is missing");
            _apiVersion = configuration["PineCone:PINECONE_API_VERSION"] ?? "2025-10";
            _model = configuration["PineCone:PINECONE_EMBED_MODEL"] ?? "llama-text-embed-v2";

            var configuredDimension = configuration["PineCone:PINECONE_EMBED_DIMENSION"];
            if (int.TryParse(configuredDimension, out var dimension) && dimension > 0)
            {
                _dimension = dimension;
            }
        }

        public async Task<float[]> GetEmbeddingAsync(string text, string inputType = "passage")
        {
            var results = await GetEmbeddingsAsync(new List<string> { text }, inputType);
            return results.FirstOrDefault() ?? Array.Empty<float>();
        }

        public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts, string inputType = "passage")
        {
            var normalizedInputType = NormalizeInputType(inputType);

            var parameters = new Dictionary<string, object>
            {
                ["input_type"] = normalizedInputType,
                ["truncate"] = "END"
            };

            if (_dimension.HasValue)
            {
                parameters["dimension"] = _dimension.Value;
            }

            var requestBody = new
            {
                model = _model,
                inputs = texts.Select(t => new { text = t }).ToList(),
                parameters
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.pinecone.io/embed")
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Api-Key", _apiKey);
            request.Headers.Add("X-Pinecone-Api-Version", _apiVersion);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Pinecone embed failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {content}");
            }

            var result = JsonConvert.DeserializeObject<PineconeEmbeddingResponse>(content);

            if (result?.Data == null || result.Data.Count == 0)
            {
                throw new InvalidOperationException("Pinecone embed returned empty data.");
            }

            return result?.Data?.Select(d => d.Values).ToList() ?? new List<float[]>();
        }

        private static string NormalizeInputType(string inputType)
        {
            if (string.Equals(inputType, "query", StringComparison.OrdinalIgnoreCase))
            {
                return "query";
            }

            if (string.Equals(inputType, "passage", StringComparison.OrdinalIgnoreCase))
            {
                return "passage";
            }

            throw new ArgumentException("inputType must be either 'query' or 'passage'.", nameof(inputType));
        }

        private class PineconeEmbeddingResponse
        {
            public List<PineconeEmbeddingData>? Data { get; set; }
        }

        private class PineconeEmbeddingData
        {
            public float[] Values { get; set; } = Array.Empty<float>();
        }
    }
}
