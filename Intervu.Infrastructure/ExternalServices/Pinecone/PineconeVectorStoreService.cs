using Microsoft.Extensions.Configuration;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;

namespace Intervu.Infrastructure.ExternalServices.Pinecone
{
    public class PineconeVectorStoreService : IVectorStoreService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiVersion;
        private readonly string _indexHost;
        private readonly string _namespace;

        public PineconeVectorStoreService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["PineCone:PINECONE_API_KEY"] ?? throw new ArgumentNullException("Pinecone API Key is missing");
            _apiVersion = configuration["PineCone:PINECONE_API_VERSION"] ?? "2025-10";

            var configuredNamespace = configuration["PineCone:PINECONE_NAMESPACE"];
            _namespace = string.IsNullOrWhiteSpace(configuredNamespace)
                ? "__default__"
                : configuredNamespace.Trim();

            var rawHost = configuration["PineCone:PINECONE_HOST_URL"] ?? throw new ArgumentNullException("Pinecone Host URL is missing");
            rawHost = rawHost.Trim().TrimEnd('/');
            _indexHost = rawHost.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? rawHost
                : $"https://{rawHost}";
        }

        public async Task UpsertAsync(string id, float[] vector, Dictionary<string, string> metadata)
        {
            var body = new
            {
                vectors = new[]
                {
                    new
                    {
                        id,
                        values = vector,
                        metadata = metadata ?? new Dictionary<string, string>()
                    }
                },
                @namespace = _namespace
            };

            await SendAsync(HttpMethod.Post, "vectors/upsert", body);
        }

        public async Task<List<VectorMatch>> SearchAsync(float[] queryVector, int topK = 5)
        {
            var body = new
            {
                vector = queryVector,
                topK,
                includeMetadata = true,
                @namespace = _namespace
            };

            var content = await SendAsync(HttpMethod.Post, "query", body);
            var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(content);

            if (queryResponse?.Matches == null) return new List<VectorMatch>();

            return queryResponse.Matches.Select(m => new VectorMatch(
                m.Id,
                m.Score,
                m.Metadata?.ToDictionary(
                    k => k.Key,
                    v => v.Value?.Type == JTokenType.Null ? string.Empty : v.Value?.ToString() ?? string.Empty)
            )).ToList();
        }

        public async Task DeleteAsync(string id)
        {
            var body = new
            {
                ids = new[] { id },
                @namespace = _namespace
            };

            await SendAsync(HttpMethod.Post, "vectors/delete", body);
        }

        private async Task<string> SendAsync(HttpMethod method, string path, object body)
        {
            var request = new HttpRequestMessage(method, $"{_indexHost}/{path}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Api-Key", _apiKey);
            request.Headers.Add("X-Pinecone-Api-Version", _apiVersion);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Pinecone request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Endpoint: {path}. Response: {content}");
            }

            return content;
        }

        private sealed class QueryResponse
        {
            [JsonProperty("matches")]
            public List<QueryMatch>? Matches { get; set; }
        }

        private sealed class QueryMatch
        {
            [JsonProperty("id")]
            public string Id { get; set; } = string.Empty;

            [JsonProperty("score")]
            public double Score { get; set; }

            [JsonProperty("metadata")]
            public Dictionary<string, JToken>? Metadata { get; set; }
        }
    }
}
