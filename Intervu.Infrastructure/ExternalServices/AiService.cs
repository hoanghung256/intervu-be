using Intervu.Application.Interfaces.ExternalServices;
using System.Net.Http.Json;
using System;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Intervu.Application.DTOs.Ai;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.SmartSearch;
using Intervu.Application.DTOs.SmartSearch;
using System.Linq;
using Microsoft.Extensions.Logging;
using Intervu.Application.DTOs;
using Intervu.Application.DTOs.Assessment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;

namespace Intervu.Infrastructure.ExternalServices
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IGetDuplicateQuestion _getDuplicateQuestion;
        private readonly ILogger<AiService> _logger;
        private readonly IAiTrafficLogRepository _aiTrafficLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AiService(
            IHttpClientFactory httpClientFactory,
            IGetDuplicateQuestion getDuplicateQuestion,
            ILogger<AiService> logger,
            IAiTrafficLogRepository aiTrafficLogRepository,
            IUnitOfWork unitOfWork)
        {
            _httpClient = httpClientFactory.CreateClient("AiServiceClient");
            _getDuplicateQuestion = getDuplicateQuestion;
            _logger = logger;
            _aiTrafficLogRepository = aiTrafficLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> StoreCvUrlAsync(Guid roomId, string cvUrl, IFormFile? file, string? useCase = null)
        {
            if (_httpClient.BaseAddress == null)
            {
                return false;
            }

            var endpoint = $"api/last-cv-pdf-url?id={roomId}";

            var swUrl = Stopwatch.StartNew();
            var response = await _httpClient.PostAsJsonAsync(endpoint, cvUrl);
            swUrl.Stop();
            var urlBody = await SafeReadStringAsync(response);
            await LogUsageAsync(ExtractUsage(urlBody), "api/last-cv-pdf-url", "HuggingFace", swUrl.ElapsedMilliseconds, useCase);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            if (file != null && file.Length > 0)
            {
                try
                {
                    var extractEndpoint = $"api/extract-cv?id={roomId}";
                    using var form = new MultipartFormDataContent();
                    await using var stream = file.OpenReadStream();
                    using var fileContent = new StreamContent(stream);
                    var mediaType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/pdf" : file.ContentType;
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                    form.Add(fileContent, "file", file.FileName);

                    var swExtract = Stopwatch.StartNew();
                    using var extractResponse = await _httpClient.PostAsync(extractEndpoint, form);
                    swExtract.Stop();
                    var extractBody = await SafeReadStringAsync(extractResponse);
                    await LogUsageAsync(ExtractUsage(extractBody), "api/extract-cv", "HuggingFace", swExtract.ElapsedMilliseconds, useCase);

                    if (!extractResponse.IsSuccessStatusCode)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<string?> GetLastCvPdfUrlAsync(Guid roomId)
        {
            if (_httpClient.BaseAddress == null)
            {
                return null;
            }

            var endpoint = $"api/last-cv-pdf-url?id={roomId}";
            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.String)
                {
                    return doc.RootElement.GetString();
                }

                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("error", out var errorProp) &&
                        errorProp.ValueKind == JsonValueKind.String &&
                        errorProp.GetString() == "No CV PDF URL has been saved yet")
                    {
                        return null;
                    }

                    if (doc.RootElement.TryGetProperty("cv_pdf_url", out var cvUrlProp) &&
                        cvUrlProp.ValueKind == JsonValueKind.String)
                    {
                        return cvUrlProp.GetString();
                    }
                }
            }
            catch
            {
                // Fallback to raw string
            }

            return raw;
        }

        public async Task<AiQuestionExtractionResponse> GetNewQuestionsFromTranscriptAsync(byte[] audioData, Guid roomId, IEnumerable<string>? availableTags = null, string? useCase = null)
        {
            if (_httpClient.BaseAddress == null)
            {
                return new AiQuestionExtractionResponse { Status = "failed", Error = "AI service not configured" };
            }

            if (audioData == null || audioData.Length == 0)
            {
                return new AiQuestionExtractionResponse { Status = "failed", Error = "Audio data is empty" };
            }

            var endpoint = $"api/transcript?id={roomId}";
            using var form = new MultipartFormDataContent();

            if (availableTags != null && availableTags.Any())
            {
                var tagsJson = System.Text.Json.JsonSerializer.Serialize(availableTags);
                form.Add(new StringContent(tagsJson), "tags");
            }

            var fileContent = new ByteArrayContent(audioData);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
            form.Add(fileContent, "file", "audio.wav");

            try
            {
                var sw = Stopwatch.StartNew();
                var response = await _httpClient.PostAsync(endpoint, form);
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(jsonResponse))
                    {
                        return new AiQuestionExtractionResponse { Status = "failed", Error = "No transcription found" };
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var result = System.Text.Json.JsonSerializer.Deserialize<AiQuestionExtractionResponse>(jsonResponse, options);

                    if (result == null)
                    {
                        return new AiQuestionExtractionResponse { Status = "failed", Error = "Failed to deserialize response" };
                    }
                    
                    if (string.IsNullOrWhiteSpace(result.Transcript))
                    {
                        result.Status = "failed";
                        result.Error = "No transcription found";
                        return result;
                    }

                    if (result.QuestionList == null || result.QuestionList.Count == 0)
                    {
                        result.Status = "failed";
                        result.Error = "Extract new question failed";
                        return result;
                    }

                    // Loop through questions and filter based on similarity
                    var validQuestions = new List<AiQuestionDto>();
                    foreach (var question in result.QuestionList)
                    {
                        var searchQuery = $"Title: {question.Title}. Content: {question.Content}";
                        var topK = 5;
                        List<QuestionSmartSearchResultDto> searchResults;

                        // Increment topK if results are all highly similar
                        while (true)
                        {
                            searchResults = await _getDuplicateQuestion.ExecuteAsync(new QuestionSmartSearchRequestDto
                            {
                                Query = searchQuery,
                                TopK = topK,
                                EntityType = "question"
                            });

                            if (!searchResults.Any())
                                break;

                            if (searchResults.Count == topK && searchResults.All(r => r.MatchScore >= 0.5))
                            {
                                topK += 10;
                                if (topK > 50) break; // Safety break
                            }
                            else
                            {
                                break;
                            }
                        }

                        // Tiered similarity logic
                        var hasHighSimilarityMatch = searchResults.Any(r => r.MatchScore > 0.7);

                        if (hasHighSimilarityMatch)
                        {
                            // Exclude immediately, do nothing else.
                            continue;
                        }

                        var mediumSimilarityMatches = searchResults.Where(r => r.MatchScore >= 0.5).ToList();

                        if (mediumSimilarityMatches.Any())
                        {
                            var payload = new
                            {
                                SimilarMatchQuestionList = mediumSimilarityMatches.Select(m => new
                                {
                                    Title = m.Question?.Title ?? string.Empty,
                                    Content = m.Question?.Content ?? string.Empty
                                }).ToList(),
                                Question = new
                                {
                                    Title = question.Title,
                                    Content = question.Content
                                }
                            };

                            try
                            {
                                var similarityEndpoint = "api/check-similarity";
                                var similarityResponse = await _httpClient.PostAsJsonAsync(similarityEndpoint, payload);

                                if (similarityResponse.IsSuccessStatusCode)
                                {
                                    var similarityJson = await similarityResponse.Content.ReadAsStringAsync();
                                    using var doc = JsonDocument.Parse(similarityJson);
                                    
                                    if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.EnumerateObject().Any())
                                    {
                                        validQuestions.Add(question);
                                    }
                                }
                                else
                                {
                                    var errorContent = await similarityResponse.Content.ReadAsStringAsync();
                                    _logger.LogError("Similarity check API failed with status {StatusCode}: {Response}", similarityResponse.StatusCode, errorContent);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An exception occurred during the similarity check API call.");
                            }
                        }
                        else
                        {
                            validQuestions.Add(question);
                        }
                    }

                    result.QuestionList = validQuestions;

                    await LogUsageAsync(result.Usage, "api/transcript", "HuggingFace", sw.ElapsedMilliseconds, useCase);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new AiQuestionExtractionResponse { Status = "failed", Error = $"Error from AI service: {response.StatusCode} - {errorContent}" };
                }
            }
            catch (Exception ex)
            {
                return new AiQuestionExtractionResponse { Status = "failed", Error = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<GenerateAssessmentResponse> GenerateAssessmentAsync(GenerateAssessmentRequest request, string? useCase = null)
        {
            if (request == null)
            {
                return new GenerateAssessmentResponse();
            }

            var sw = Stopwatch.StartNew();
            var response = await _httpClient.PostAsJsonAsync("api/generate-assessment", request);
            sw.Stop();

            var rawContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"AI service request failed with status code {(int)response.StatusCode}: {rawContent}");
            }

            GenerateAssessmentResponse? result = null;

            if (!string.IsNullOrWhiteSpace(rawContent))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<GenerateAssessmentResponse>(rawContent);

                    if (result != null && string.IsNullOrWhiteSpace(result.ContextQuestion))
                    {
                        var root = JObject.Parse(rawContent);
                        result.ContextQuestion = root.Value<string>("context_question")
                            ?? root.Value<string>("contextQuestion")
                            ?? string.Empty;
                    }
                }
                catch (Newtonsoft.Json.JsonException)
                {
                    result = null;
                }
            }

            if (result == null)
            {
                result = new GenerateAssessmentResponse();
            }

            result.PhaseA ??= new JArray();
            result.PhaseB ??= new JArray();

            var usage = result.Usage ?? ExtractUsage(rawContent);
            await LogUsageAsync(usage, "api/generate-assessment", "HuggingFace", sw.ElapsedMilliseconds, useCase);
            return result;
        }

        public async Task<AiGenerateRoadmapResponseDto?> GenerateRoadmapAsync(AiGenerateRoadmapRequestDto request, CancellationToken cancellationToken = default, string? useCase = null)
        {
            if (_httpClient.BaseAddress == null)
            {
                return null;
            }

            _logger.LogInformation("Calling AI generate-roadmap");

            var sw = Stopwatch.StartNew();
            var response = await _httpClient.PostAsJsonAsync("api/generate-roadmap", request, cancellationToken);
            sw.Stop();
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("AI generate-roadmap failed with status {StatusCode}: {Body}", response.StatusCode, rawContent);
                return new AiGenerateRoadmapResponseDto
                {
                    Status = "failed",
                    Error = $"AI service error: {(int)response.StatusCode}"
                };
            }

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                _logger.LogWarning("AI generate-roadmap returned an empty body");
                return new AiGenerateRoadmapResponseDto
                {
                    Status = "failed",
                    Error = "Empty response from AI roadmap service"
                };
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            try
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<AiGenerateRoadmapResponseDto>(rawContent, options);
                _logger.LogInformation("AI generate-roadmap returned status {Status}", result?.Status);
                await LogUsageAsync(result?.Usage, "api/generate-roadmap", "Gemini", sw.ElapsedMilliseconds, useCase);
                return result;
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize AI roadmap response");
                return new AiGenerateRoadmapResponseDto
                {
                    Status = "failed",
                    Error = "Invalid roadmap payload format from AI roadmap service"
                };
            }
        }

        public async Task<AiUpdateRoadmapProgressResponseDto?> UpdateRoadmapProgressAsync(AiUpdateRoadmapProgressRequestDto request, CancellationToken cancellationToken = default, string? useCase = null)
        {
            if (_httpClient.BaseAddress == null)
            {
                return null;
            }

            _logger.LogInformation("Calling AI update-roadmap-progress");

            var sw = Stopwatch.StartNew();
            var response = await _httpClient.PostAsJsonAsync("api/update-roadmap-progress", request, cancellationToken);
            sw.Stop();
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("AI update-roadmap-progress failed with status {StatusCode}: {Body}", response.StatusCode, rawContent);
                return new AiUpdateRoadmapProgressResponseDto
                {
                    Status = "failed",
                    Error = $"AI service error: {(int)response.StatusCode}"
                };
            }

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                _logger.LogWarning("AI update-roadmap-progress returned an empty body");
                return new AiUpdateRoadmapProgressResponseDto
                {
                    Status = "failed",
                    Error = "Empty response from AI roadmap progress service"
                };
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            try
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<AiUpdateRoadmapProgressResponseDto>(rawContent, options);
                _logger.LogInformation("AI update-roadmap-progress returned status {Status}", result?.Status);
                await LogUsageAsync(ExtractUsage(rawContent), "api/update-roadmap-progress", "Gemini", sw.ElapsedMilliseconds, useCase);
                return result;
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize roadmap progress response");
                return new AiUpdateRoadmapProgressResponseDto
                {
                    Status = "failed",
                    Error = "Invalid payload format from AI roadmap progress service"
                };
            }
        }

        private async Task LogUsageAsync(LlmTokenUsageDto? usage, string endpointName, string provider, long latencyMs, string? useCase = null)
        {
            try
            {
                var log = new AiTrafficLog
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EndpointName = endpointName,
                    UseCase = useCase ?? string.Empty,
                    Provider = provider,
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

        public async Task<AiCvEvaluationResponseDto?> EvaluateCvAsync(System.IO.Stream stream, string fileName, string contentType, string? useCase = null)
        {
            if (_httpClient.BaseAddress == null || stream == null)
            {
                return null;
            }

            try
            {
                var endpoint = "api/evaluate-cv";
                using var form = new MultipartFormDataContent();

                // Note: We don't dispose the stream here as it's passed in from outside
                using var fileContent = new StreamContent(stream);

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                form.Add(fileContent, "file", fileName);

                var sw = Stopwatch.StartNew();
                var response = await _httpClient.PostAsync(endpoint, form);
                sw.Stop();

                var rawContent = await SafeReadStringAsync(response);
                await LogUsageAsync(ExtractUsage(rawContent), endpoint, "HuggingFace", sw.ElapsedMilliseconds, useCase);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("AI service evaluation request failed with status code {StatusCode}", response.StatusCode);
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                return System.Text.Json.JsonSerializer.Deserialize<AiCvEvaluationResponseDto>(rawContent, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while evaluating CV via AI service.");
                return null;
            }
        }

        private static async Task<string> SafeReadStringAsync(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static LlmTokenUsageDto? ExtractUsage(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(rawJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                if (!doc.RootElement.TryGetProperty("usage", out var usageEl) ||
                    usageEl.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                int prompt = 0;
                int completion = 0;
                int total = 0;
                if (usageEl.TryGetProperty("prompt_tokens", out var p) && p.TryGetInt32(out var pi)) prompt = pi;
                if (usageEl.TryGetProperty("completion_tokens", out var c) && c.TryGetInt32(out var ci)) completion = ci;
                if (usageEl.TryGetProperty("total_tokens", out var t) && t.TryGetInt32(out var ti)) total = ti;

                return new LlmTokenUsageDto
                {
                    PromptTokens = prompt,
                    CompletionTokens = completion,
                    TotalTokens = total,
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
