using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Intervu.Application.DTOs.SmartSearch;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.UseCases.SmartSearch;

namespace Intervu.Application.UseCases.SmartSearch
{
    public class SmartSearchExtractDataFromFile : ISmartSearchExtractDataFromFile
    {
        private readonly IPythonAiService _pythonAiService;

        public SmartSearchExtractDataFromFile(IPythonAiService pythonAiService)
        {
            _pythonAiService = pythonAiService;
        }

        public async Task<string> ExecuteAsync(SmartSearchExtractRequestDto request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ArgumentException("File is required and cannot be empty.");
            }

            using var stream = request.File.OpenReadStream();

            var jsonResponse = await _pythonAiService.ExtractDocumentToJsonAsync(
                stream,
                request.File.FileName,
                request.DocType);

            // Inject a distilled search_profile into the raw extraction result.
            // This gives the frontend a compact, editable profile for the user to review.
            return InjectSearchProfile(jsonResponse, request.DocType);
        }

        /// <summary>
        /// Parses the raw extraction JSON, builds a compact search profile with only
        /// search-critical fields, and injects it as "search_profile" before returning.
        /// </summary>
        private static string InjectSearchProfile(string rawJson, string docType)
        {
            try
            {
                var root = JsonNode.Parse(rawJson)?.AsObject();
                if (root == null) return rawJson;

                var profile = new JsonObject();

                if (docType.Equals("jd", StringComparison.OrdinalIgnoreCase))
                {
                    CopyIfPresent(root, "job_title", profile, "job_title");
                    CopyIfPresent(root, "must_have_skills", profile, "must_have_skills");
                    CopyIfPresent(root, "nice_to_have_skills", profile, "nice_to_have_skills");
                    CopyIfPresent(root, "required_yoe", profile, "required_yoe");
                    // Intentionally excluded: core_responsibilities, benefits, company_culture
                }
                else
                {
                    // CV: extract target role from apply_for
                    var applyFor = root["apply_for"];
                    if (applyFor != null)
                    {
                        var jobTitle = applyFor is JsonObject applyObj
                            ? applyObj["job_title"]?.GetValue<string>()
                            : applyFor.GetValue<string>();
                        if (!string.IsNullOrWhiteSpace(jobTitle))
                            profile["target_role"] = jobTitle;
                    }

                    CopyIfPresent(root, "skills", profile, "skills");
                    CopyIfPresent(root, "total_years_of_experience", profile, "total_years_of_experience");

                    // Experiences: keep only company + title
                    var experiences = root["experiences"]?.AsArray();
                    if (experiences != null && experiences.Count > 0)
                    {
                        var slim = new JsonArray();
                        foreach (var exp in experiences)
                        {
                            if (exp is not JsonObject expObj) continue;
                            var entry = new JsonObject();
                            if (expObj["company"] != null)
                                entry["company"] = JsonNode.Parse(expObj["company"]!.ToJsonString());
                            if (expObj["title"] != null)
                                entry["title"] = JsonNode.Parse(expObj["title"]!.ToJsonString());
                            if (entry.Count > 0) slim.Add(entry);
                        }
                        if (slim.Count > 0) profile["experiences"] = slim;
                    }
                    // Intentionally excluded: languages, certifications, dates, descriptions
                }

                root["search_profile"] = profile;
                return root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            }
            catch
            {
                // If parsing fails, return the raw JSON unchanged
                return rawJson;
            }
        }

        /// <summary>
        /// Copies a field from source to target only if it has meaningful content.
        /// Skips null, empty strings, empty arrays, and zero/null numerics.
        /// </summary>
        private static void CopyIfPresent(JsonObject source, string sourceKey, JsonObject target, string targetKey)
        {
            var node = source[sourceKey];
            if (node == null) return;

            // Skip empty arrays
            if (node is JsonArray arr && arr.Count == 0) return;

            // Skip empty/whitespace strings
            if (node is JsonValue val && val.TryGetValue<string>(out var str) && string.IsNullOrWhiteSpace(str)) return;

            target[targetKey] = JsonNode.Parse(node.ToJsonString());
        }
    }
}
