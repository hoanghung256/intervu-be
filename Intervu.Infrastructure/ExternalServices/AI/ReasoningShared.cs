using Intervu.Application.Interfaces.ExternalServices.AI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    internal static class ReasoningShared
    {
        internal static string BuildPrompt(string query, List<ReasoningCandidate> candidates)
        {
            var candidatesJson = JsonConvert.SerializeObject(candidates);
            var idList = string.Join(", ", candidates.Select(c => c.Id));

            return $@"You are an expert AI mentor-matching assistant.

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
{candidatesJson}

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

MANDATORY OUTPUT RULES:
1. You MUST return EXACTLY {candidates.Count} items — one for EVERY coach ID listed below. Do NOT skip or omit any ID.
2. Required IDs: {idList}
3. Copy each ""id"" value exactly as shown above (lowercase, with hyphens). Do NOT change ID casing or format.
4. Reasoning must be concise (max 3 sentences), direct, and in third-person style (use ""you""/""your"").
5. Mention the coach's key matching skills by name, and briefly explain how the coach can help fill your 
gaps toward the target role.
6. Each reasoning must reference at least two concrete signals from input (skills/domain/seniority/role requirements).
7. If mismatch is large, explicitly name the key skill or domain gaps.
8. Do not invent facts not present in the input.
9. Return ONLY a raw JSON array. No markdown code fences, no wrapper object.
10.NEVER include or reveal any identifier in reasoning text: coach ID, user ID, UUID, slug, email, or internal key.

Required JSON schema:
[
  {{ ""id"": ""string"", ""score"": 0.0, ""reasoning"": ""string"" }}
]";
        }

        internal static List<ReasoningResult> ParseResults(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<ReasoningResult>();

            var cleaned = json.Trim();

            // Strip markdown code fences if present (```json ... ```)
            var fenceMatch = Regex.Match(cleaned, @"```(?:json)?\s*([\s\S]*?)\s*```");
            if (fenceMatch.Success)
            {
                cleaned = fenceMatch.Groups[1].Value.Trim();
            }

            // Try parsing as-is first
            var results = TryParseJson(cleaned);
            if (results != null) return results;

            // Attempt to recover truncated JSON array by closing it
            if (cleaned.StartsWith("[") && !cleaned.EndsWith("]"))
            {
                // Find the last complete object (ending with })
                var lastBrace = cleaned.LastIndexOf('}');
                if (lastBrace > 0)
                {
                    var recovered = cleaned.Substring(0, lastBrace + 1) + "]";
                    results = TryParseJson(recovered);
                    if (results != null) return results;
                }
            }

            return new List<ReasoningResult>();
        }

        private static List<ReasoningResult>? TryParseJson(string json)
        {
            try
            {
                var token = JToken.Parse(json);
                if (token.Type == JTokenType.Array)
                {
                    return token.ToObject<List<ReasoningResult>>() ?? new List<ReasoningResult>();
                }

                if (token.Type == JTokenType.Object)
                {
                    var obj = (JObject)token;
                    var wrapped = obj["results"] ?? obj["data"];
                    if (wrapped is JArray arr)
                    {
                        return arr.ToObject<List<ReasoningResult>>() ?? new List<ReasoningResult>();
                    }

                    var single = obj.ToObject<ReasoningResult>();
                    return single == null ? null : new List<ReasoningResult> { single };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
