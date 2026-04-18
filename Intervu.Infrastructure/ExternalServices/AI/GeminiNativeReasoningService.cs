using Intervu.Application.Interfaces.ExternalServices.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    public class GeminiNativeReasoningService : ISmartSearchReasoningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiNativeReasoningService> _logger;
        private const string DEFAULT_MODEL = "gemini-3.1-flash-lite-preview";
        private const string DEFAULT_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models";

        public GeminiNativeReasoningService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiNativeReasoningService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<ReasoningResult>> RerankAndReasonAsync(string query, List<ReasoningCandidate> candidates, string? useCase = null)
        {
            if (!candidates.Any()) return new List<ReasoningResult>();

            if (!_configuration.GetValue<bool>("SmartSearch:LlmRerankEnabled", true))
            {
                return new List<ReasoningResult>();
            }

            var apiKey = _configuration["GeminiApi:GEMINI_API_KEY"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new List<ReasoningResult>();
            }

            var timeoutMs = _configuration.GetValue<int>("SmartSearch:LlmTimeoutMs", 8000);
            var model = _configuration["GeminiApi:ModelId"] ?? DEFAULT_MODEL;
            var baseUrl = _configuration["GeminiApi:BaseUrl"] ?? DEFAULT_BASE_URL;
            var url = $"{baseUrl}/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = ReasoningShared.BuildPrompt(query, candidates) } }
                    }
                },
                generationConfig = new
                {
                    temperature = _configuration.GetValue<double>("GeminiApi:Temperature", 0.2),
                    responseMimeType = "application/json"
                }
            };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                var response = await _httpClient.PostAsync(
                    url,
                    new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"),
                    cts.Token);

                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini native reasoning rejected request. Status: {StatusCode}. Body: {Body}", response.StatusCode, responseBody);
                    return new List<ReasoningResult>();
                }

                var parsed = JsonConvert.DeserializeObject<GeminiResponse>(responseBody);
                var raw = parsed?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                return ReasoningShared.ParseResults(raw);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Gemini native reasoning timed out after {TimeoutMs}ms.", timeoutMs);
                return new List<ReasoningResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini native reasoning failed.");
                return new List<ReasoningResult>();
            }
        }

        private class GeminiResponse
        {
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent? Content { get; set; }
        }

        private class GeminiContent
        {
            public List<GeminiPart>? Parts { get; set; }
        }

        private class GeminiPart
        {
            public string? Text { get; set; }
        }
    }
}
