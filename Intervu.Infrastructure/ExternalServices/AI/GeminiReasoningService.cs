using Intervu.Application.Interfaces.ExternalServices.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    public class GeminiReasoningService : ISmartSearchReasoningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiReasoningService> _logger;
        private const string GEMINI_MODEL = "gemini-3-flash-preview"; // Fast and cheap reasoning model

        public GeminiReasoningService(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<GeminiReasoningService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<ReasoningResult>> RerankAndReasonAsync(string query, List<ReasoningCandidate> candidates)
        {
            if (!candidates.Any()) return new List<ReasoningResult>();

            // Feature Switch
            var isEnabled = _configuration.GetValue<bool>("SmartSearch:LlmRerankEnabled", true);
            if (!isEnabled)
            {
                _logger.LogInformation("LLM Reranking is disabled via configuration. Bypassing Gemini.");
                return new List<ReasoningResult>();
            }

            var apiKey = _configuration["GeminiApi:GEMINI_API_KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Gemini API Key is missing. Check appsettings. Bypassing reasoning.");
                return new List<ReasoningResult>();
            }

            var timeoutMs = _configuration.GetValue<int>("SmartSearch:LlmTimeoutMs", 8000); // 8 second default limit

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GEMINI_MODEL}:generateContent?key={apiKey}";

            var CandidatesJson = JsonConvert.SerializeObject(candidates);

            var systemInstruction = $@"You are an expert AI recruiting and matching assistant.
Your task is to re-evaluate and re-rank candidate profiles based on how well they match the user's explicit query.
The candidates were initially retrieved via vector similarity search, which might miss nuance. You must fix this by reasoning over their actual summaries.

USER QUERY:
""{query}""

CANDIDATES (JSON format):
{CandidatesJson}

INSTRUCTIONS:
1. Analyze each candidate against the exact requirements in the user's query.
2. Provide a new relevance 'score' between 0.0 and 1.0 (1.0 being an absolute perfect match).
3. Provide a very concise 'reasoning' (maximum 2 sentences) explaining *why* this candidate is a good fit tailored exactly to the user's query. Do not summarize their entire profile, focus only on the match.
4. If a candidate is a completely irrelevant match, give them a score below 0.3.
5. Return the result strictly matching the provided JSON schema.
";

            // Enforce strictly typed JSON schema for guaranteed parsable output
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = systemInstruction } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2, // Low temp for more deterministic reasoning
                    responseMimeType = "application/json",
                    responseSchema = new
                    {
                        type = "ARRAY",
                        description = "List of re-ranked and reasoned results.",
                        items = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                id = new { type = "STRING", description = "The ID of the candidate matching exactly the input array." },
                                score = new { type = "NUMBER", description = "The relevance score between 0.0 and 1.0." },
                                reasoning = new { type = "STRING", description = "A concise, 1-2 sentence compelling reason linking the candidate's skills to the user's query." }
                            },
                            required = new[] { "id", "score", "reasoning" }
                        }
                    }
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                var response = await _httpClient.PostAsync(url, jsonContent, cts.Token);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseBody);

                var jsonResultText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrWhiteSpace(jsonResultText))
                {
                    _logger.LogWarning("Gemini returned an empty or unparsable structure.");
                    return new List<ReasoningResult>();
                }

                var results = JsonConvert.DeserializeObject<List<ReasoningResult>>(jsonResultText);
                return results ?? new List<ReasoningResult>();
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning($"Gemini API request timed out after {timeoutMs}ms. Fallback to pinecone vectors will occur.");
                return new List<ReasoningResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling Gemini for reasoning. Fallback to pinecone vectors will occur.");
                return new List<ReasoningResult>();
            }
        }

        // Schema mappings to deserialize the Gemini v1beta response wrapper
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
