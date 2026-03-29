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
        private const string GEMINI_MODEL = "gemini-3.1-flash-lite-preview"; // User requested flash 3

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

                        var systemInstruction = $@"You are an expert AI mentor-matching assistant.

IMPORTANT CONTEXT:
- This is NOT a hiring/recruitment ranking.
- You are selecting interview coaches for the user (candidate), not evaluating candidates for a company.
- Input text may include:
    1) user natural-language goal,
    2) extracted CV context,
    3) extracted JD context.

TASK:
Re-rank coach candidates by coaching suitability for the user's TARGET ROLE and interview goal.

USER CONTEXT:
""{query}""

COACH CANDIDATES (JSON format):
{CandidatesJson}

TARGET ROLE PRIORITY (STRICT):
1. Identify the primary target role from explicit user goal first.
2. If unclear, infer from JD role/title and requirements.
3. If still unclear, infer from CV target role (not incidental past stacks).
4. Treat past internship/legacy stacks in CV as secondary unless directly required by the target role/JD.

EVALUATION CRITERIA:
1. Target-role alignment: coach expertise is relevant to the user's target role.
2. JD alignment (if present): coach can train the skills/responsibilities required by that JD.
3. Gap-closing value: coach can help close the user's current gaps from CV context toward the target role.
4. Seniority fit: coach level is appropriate for the user's goal.
5. Practical interview value: coach profile suggests concrete interview preparation guidance.

SCORING RULES:
- Be strict and uncompromising. Do NOT inflate scores.
- If evidence is weak, missing, or ambiguous, score lower.
- 0.85-1.00: strong fit for the target role and likely high coaching value.
- 0.60-0.84: reasonable fit with notable gaps.
- 0.30-0.59: weak fit.
- below 0.30: irrelevant or off-track.
- If coach is off-track from target role/JD, score <= 0.20.
- If no coach is truly suitable, score all candidates low instead of forcing a high match.

OUTPUT STYLE RULES:
1. Return one item per relevant coach ID from the input list.
2. Reasoning must be concise (max 2 sentences), direct, and in second-person style (use ""you""/""your"").
3. Explain why this coach is or is not a fit for your target role (not generic recruiting language).
4. If mismatch is large, explicitly name the key gaps.
5. Do not invent facts not present in the input.
6. Return ONLY JSON that matches the required schema.
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

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API rejected request. Status: {StatusCode}. Body: {ErrorBody}", response.StatusCode, errorBody);
                }
                
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
