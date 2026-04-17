using Intervu.Application.Interfaces.ExternalServices.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    public class HuggingFaceReasoningService : ISmartSearchReasoningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HuggingFaceReasoningService> _logger;
        private const string DEFAULT_MODEL = "meta-llama/Llama-3.1-8B-Instruct:novita";
        private const string DEFAULT_URL = "https://router.huggingface.co/v1/chat/completions";

        public HuggingFaceReasoningService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<HuggingFaceReasoningService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<ReasoningResult>> RerankAndReasonAsync(string query, List<ReasoningCandidate> candidates)
        {
            if (!candidates.Any()) return new List<ReasoningResult>();

            if (!_configuration.GetValue<bool>("SmartSearch:LlmRerankEnabled", true))
            {
                return new List<ReasoningResult>();
            }

            var apiKey = _configuration["ReasoningApi:AI_API_KEY"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new List<ReasoningResult>();
            }

            var timeoutMs = _configuration.GetValue<int>("SmartSearch:LlmTimeoutMs", 8000);
            var url = _configuration["ReasoningApi:BaseUrl"] ?? DEFAULT_URL;
            var model = _configuration["ReasoningApi:ModelId"] ?? DEFAULT_MODEL;
            var apiKeyHeader = _configuration["ReasoningApi:ApiKeyHeader"] ?? "Authorization";
            var apiKeyPrefix = _configuration["ReasoningApi:ApiKeyPrefix"] ?? "Bearer";
            var maxTokens = _configuration.GetValue<int>("ReasoningApi:MaxTokens", 1024);
            var temperature = _configuration.GetValue<double>("ReasoningApi:Temperature", 0.2);

            var idList = string.Join(", ", candidates.Select(c => c.Id));
            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = ReasoningShared.BuildSystemPrompt(candidates.Count, idList) },
                    new { role = "user", content = ReasoningShared.BuildUserPrompt(query, candidates) }
                },
                temperature,
                max_tokens = maxTokens,
                stream = false
            };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                var headerValue = string.IsNullOrWhiteSpace(apiKeyPrefix) ? apiKey : $"{apiKeyPrefix} {apiKey}";
                request.Headers.TryAddWithoutValidation(apiKeyHeader, headerValue);
                request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cts.Token);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("HF reasoning rejected request. Status: {StatusCode}. Body: {Body}", response.StatusCode, responseBody);
                    return new List<ReasoningResult>();
                }

                var chatResponse = JsonConvert.DeserializeObject<HfChatResponse>(responseBody);
                return ReasoningShared.ParseResults(chatResponse?.Choices?.FirstOrDefault()?.Message?.Content);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("HF reasoning timed out after {TimeoutMs}ms.", timeoutMs);
                return new List<ReasoningResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HF reasoning failed.");
                return new List<ReasoningResult>();
            }
        }

        private class HfChatResponse
        {
            public List<HfChoice>? Choices { get; set; }
        }

        private class HfChoice
        {
            public HfMessage? Message { get; set; }
        }

        private class HfMessage
        {
            public string? Content { get; set; }
        }
    }
}
