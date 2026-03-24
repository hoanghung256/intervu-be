using Intervu.Application.Interfaces.ExternalServices;
using System.Net.Http.Json;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Intervu.Application.DTOs.Ai;
using Intervu.Application.DTOs.Question;

namespace Intervu.Infrastructure.ExternalServices
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;

        public AiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AiServiceClient");
        }

        public async Task<bool> StoreCvUrlAsync(Guid roomId, string cvUrl, IFormFile? file)
        {
            if (_httpClient.BaseAddress == null)
            {
                return false;
            }
            
            var endpoint = $"api/last-cv-pdf-url?id={roomId}";

            var response = await _httpClient.PostAsJsonAsync(endpoint, cvUrl);
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

                    using var extractResponse = await _httpClient.PostAsync(extractEndpoint, form);
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

        public async Task<AiQuestionExtractionResponse> GetNewQuestionsFromTranscriptAsync(byte[] audioData, Guid roomId)
        {
            if (_httpClient.BaseAddress == null)
            {
                return new AiQuestionExtractionResponse { Status = "failed", Error = "AI service not configured" };
            }
            
            var endpoint = $"api/transcript?id={roomId}";
            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(audioData);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
            form.Add(fileContent, "file", "audio.wav");

            try
            {
                var response = await _httpClient.PostAsync(endpoint, form);

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
                    var result = JsonSerializer.Deserialize<AiQuestionExtractionResponse>(jsonResponse, options);

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
    }
}
