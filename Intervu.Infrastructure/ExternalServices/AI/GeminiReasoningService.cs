using Intervu.Application.Interfaces.ExternalServices.AI;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    public class GeminiReasoningService : ISmartSearchReasoningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiReasoningService> _logger;
        private readonly IAiTrafficLogRepository _aiTrafficLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private const string HF_REASONING_MODEL = "meta-llama/Llama-3.1-8B-Instruct:novita";
        private const string HF_REASONING_URL = "https://router.huggingface.co/v1/chat/completions";

        public GeminiReasoningService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiReasoningService> logger,
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

            // Feature Switch
            var isEnabled = _configuration.GetValue<bool>("SmartSearch:LlmRerankEnabled", true);
            if (!isEnabled)
            {
                _logger.LogInformation("LLM Reranking is disabled via configuration. Bypassing reasoning.");
                return new List<ReasoningResult>();
            }

            var apiKey = _configuration["ReasoningApi:AI_API_KEY"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("HF_TOKEN");
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Reasoning API key is missing (ReasoningApi:AI_API_KEY or HF_TOKEN). Bypassing reasoning.");
                return new List<ReasoningResult>();
            }

            if (!apiKey.StartsWith("hf_", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Reasoning API key does not look like a Hugging Face token (expected prefix 'hf_'). Request may return 401.");
            }

            var timeoutMs = _configuration.GetValue<int>("SmartSearch:LlmTimeoutMs", 8000); // 8 second default limit
            var reasoningUrl = _configuration["ReasoningApi:BaseUrl"] ?? HF_REASONING_URL;
            var reasoningModel = _configuration["ReasoningApi:ModelId"] ?? HF_REASONING_MODEL;

            var idList = string.Join(", ", candidates.Select(c => c.Id));
            var requestBody = new
            {
                model = reasoningModel,
                messages = new[]
                {
                    new { role = "system", content = ReasoningShared.BuildSystemPrompt(candidates.Count, idList) },
                    new { role = "user", content = ReasoningShared.BuildUserPrompt(query, candidates) }
                },
                temperature = 0.2
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                using var request = new HttpRequestMessage(HttpMethod.Post, reasoningUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = jsonContent;

                var sw = Stopwatch.StartNew();
                var response = await _httpClient.SendAsync(request, cts.Token);
                sw.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Reasoning API rejected request. Status: {StatusCode}. Model: {Model}. Body: {ErrorBody}", response.StatusCode, reasoningModel, errorBody);
                }

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var aiResponse = JsonConvert.DeserializeObject<HuggingFaceChatResponse>(responseBody);
                _ = TryLogUsageAsync(aiResponse?.Usage, sw.ElapsedMilliseconds, useCase);

                var rawText = aiResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                var jsonResultText = ExtractJsonPayload(rawText) ?? rawText;

                if (string.IsNullOrWhiteSpace(jsonResultText))
                {
                    _logger.LogWarning("Reasoning API returned an empty or unparsable structure.");
                    return new List<ReasoningResult>();
                }

                var results = ParseReasoningResults(jsonResultText);
                return results ?? new List<ReasoningResult>();
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning($"Reasoning API request timed out after {timeoutMs}ms. Fallback to pinecone vectors will occur.");
                return new List<ReasoningResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling Reasoning API for reasoning. Fallback to pinecone vectors will occur.");
                return new List<ReasoningResult>();
            }
        }

        private static string? ExtractJsonPayload(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var text = content.Trim();
            if (text.StartsWith("```"))
            {
                text = text.Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
                           .Replace("```", string.Empty)
                           .Trim();
            }

            var arrayStart = text.IndexOf('[');
            var objectStart = text.IndexOf('{');
            var start = -1;

            if (arrayStart >= 0 && objectStart >= 0)
            {
                start = Math.Min(arrayStart, objectStart);
            }
            else if (arrayStart >= 0)
            {
                start = arrayStart;
            }
            else if (objectStart >= 0)
            {
                start = objectStart;
            }

            if (start < 0) return null;

            var depth = 0;
            var inString = false;
            var escaped = false;
            var openChar = text[start];
            var closeChar = openChar == '[' ? ']' : '}';

            for (var i = start; i < text.Length; i++)
            {
                var ch = text[i];

                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                        continue;
                    }

                    if (ch == '\\')
                    {
                        escaped = true;
                        continue;
                    }

                    if (ch == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == openChar) depth++;
                if (ch == closeChar)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text[start..(i + 1)];
                    }
                }
            }

            return null;
        }

        private async Task TryLogUsageAsync(HuggingFaceUsage? usage, long latencyMs, string? useCase = null)
        {
            try
            {
                var log = new AiTrafficLog
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EndpointName = "gemini-rerank",
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

        private static List<ReasoningResult> ParseReasoningResults(string json)
        {
            var token = JToken.Parse(json);

            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<ReasoningResult>>() ?? new List<ReasoningResult>();
            }

            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;

                // Common wrapped formats: { results: [...] } or { data: [...] }
                var wrappedArray = obj["results"] ?? obj["data"];
                if (wrappedArray is JArray arr)
                {
                    return arr.ToObject<List<ReasoningResult>>() ?? new List<ReasoningResult>();
                }

                // Single-object fallback: { id/Id, score/Score, reasoning/Reasoning }
                var single = obj.ToObject<ReasoningResult>();
                return single == null ? new List<ReasoningResult>() : new List<ReasoningResult> { single };
            }

            return new List<ReasoningResult>();
        }

        // Schema mappings to deserialize OpenAI-compatible response wrapper
        private class HuggingFaceChatResponse
        {
            public List<HuggingFaceChoice>? Choices { get; set; }

            [JsonProperty("usage")]
            public HuggingFaceUsage? Usage { get; set; }
        }

        private class HuggingFaceChoice
        {
            public HuggingFaceMessage? Message { get; set; }
        }

        private class HuggingFaceMessage
        {
            public string? Content { get; set; }
        }

        private class HuggingFaceUsage
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
