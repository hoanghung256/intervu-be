using Intervu.Application.Interfaces.ExternalServices.AI;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    public class HuggingFaceReasoningService : ISmartSearchReasoningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HuggingFaceReasoningService> _logger;
        private readonly IAiTrafficLogRepository _aiTrafficLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private const string DEFAULT_MODEL = "meta-llama/Llama-3.1-8B-Instruct:novita";
        private const string DEFAULT_URL = "https://router.huggingface.co/v1/chat/completions";

        public HuggingFaceReasoningService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<HuggingFaceReasoningService> logger,
            IAiTrafficLogRepository aiTrafficLogRepository,
            IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _aiTrafficLogRepository = aiTrafficLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ReasoningResult>> RerankAndReasonAsync(string query, List<ReasoningCandidate> candidates, string? useCase = null)
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
            // 3072 tokens fits ~10 candidates with reasoning strings without truncation.
            // Prior default (1024) truncated responses when candidate count grew, causing empty parses.
            var maxTokens = _configuration.GetValue<int>("ReasoningApi:MaxTokens", 3072);
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

                var sw = Stopwatch.StartNew();
                var response = await _httpClient.SendAsync(request, cts.Token);
                sw.Stop();
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("HF reasoning rejected request. Status: {StatusCode}. Body: {Body}", response.StatusCode, responseBody);
                    return new List<ReasoningResult>();
                }

                var chatResponse = JsonConvert.DeserializeObject<HfChatResponse>(responseBody);

                _ = TryLogUsageAsync(chatResponse?.Usage, sw.ElapsedMilliseconds, useCase);

                var rawContent = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                var parsed = ReasoningShared.ParseResults(rawContent);

                _logger.LogInformation(
                    "HF reasoning parsed {ParsedCount}/{ExpectedCount} results (model={Model}, rawLen={RawLen})",
                    parsed.Count, candidates.Count, model, rawContent?.Length ?? 0);

                if (parsed.Count == 0 && !string.IsNullOrWhiteSpace(rawContent))
                {
                    _logger.LogWarning("HF reasoning returned non-empty content but parser found 0 results. First 500 chars: {Snippet}",
                        rawContent.Length > 500 ? rawContent[..500] : rawContent);
                }

                return parsed;
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

        private async Task TryLogUsageAsync(HfUsage? usage, long latencyMs, string? useCase = null)
        {
            try
            {
                var log = new AiTrafficLog
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EndpointName = "smart-search-rerank",
                    UseCase = useCase ?? string.Empty,
                    Provider = "HuggingFace",
                    PromptTokens = usage?.PromptTokens ?? 0,
                    CompletionTokens = usage?.CompletionTokens ?? 0,
                    LatencyMs = latencyMs,
                };
                await _aiTrafficLogRepository.AddAsync(log);
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                // Logging must never break the main flow
            }
        }

        private class HfChatResponse
        {
            public List<HfChoice>? Choices { get; set; }

            [JsonProperty("usage")]
            public HfUsage? Usage { get; set; }
        }

        private class HfChoice
        {
            public HfMessage? Message { get; set; }
        }

        private class HfMessage
        {
            public string? Content { get; set; }
        }

        private class HfUsage
        {
            [JsonProperty("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonProperty("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonProperty("total_tokens")]
            public int TotalTokens { get; set; }
        }
    }
}
