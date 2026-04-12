using Intervu.Application.Interfaces.ExternalServices.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    public class GeminiReasoningService : ISmartSearchReasoningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiReasoningService> _logger;
        private const string HF_REASONING_MODEL = "meta-llama/Llama-3.1-8B-Instruct:novita";
        private const string HF_REASONING_URL = "https://router.huggingface.co/v1/chat/completions";

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
7. Return raw JSON only. No markdown code fences.
";

            var requestBody = new
            {
                model = reasoningModel,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = systemInstruction
                    }
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
                var response = await _httpClient.SendAsync(request, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Reasoning API rejected request. Status: {StatusCode}. Model: {Model}. Body: {ErrorBody}", response.StatusCode, reasoningModel, errorBody);
                }
                
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var aiResponse = JsonConvert.DeserializeObject<HuggingFaceChatResponse>(responseBody);

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
        }

        private class HuggingFaceChoice
        {
            public HuggingFaceMessage? Message { get; set; }
        }

        private class HuggingFaceMessage
        {
            public string? Content { get; set; }
        }
    }
}
