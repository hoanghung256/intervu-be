using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices.AI
{
    public class PythonAiService : IPythonAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly IAiTrafficLogRepository _aiTrafficLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PythonAiService(
            HttpClient httpClient,
            IConfiguration configuration,
            IAiTrafficLogRepository aiTrafficLogRepository,
            IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["PythonAiService:BaseUrl"] ?? "http://localhost:8000";
            _aiTrafficLogRepository = aiTrafficLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> ExtractDocumentToJsonAsync(Stream fileStream, string fileName, string docType)
        {
            var url = $"{_baseUrl}/api/extract-document";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            using var content = new MultipartFormDataContent();

            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(streamContent, "file", fileName);
            content.Add(new StringContent(docType), "doc_type");

            request.Content = content;

            var sw = Stopwatch.StartNew();
            var response = await _httpClient.SendAsync(request);
            sw.Stop();
            response.EnsureSuccessStatusCode();

            var rawJson = await response.Content.ReadAsStringAsync();

            await LogUsageAsync(rawJson, "extract-document", "HuggingFace", sw.ElapsedMilliseconds);

            return rawJson;
        }

        private async Task LogUsageAsync(string rawJson, string endpointName, string provider, long latencyMs)
        {
            try
            {
                int promptTokens = 0, completionTokens = 0;
                if (!string.IsNullOrWhiteSpace(rawJson))
                {
                    using var doc = JsonDocument.Parse(rawJson);
                    if (doc.RootElement.TryGetProperty("usage", out var usageEl))
                    {
                        if (usageEl.TryGetProperty("prompt_tokens", out var pt)) promptTokens = pt.GetInt32();
                        if (usageEl.TryGetProperty("completion_tokens", out var ct)) completionTokens = ct.GetInt32();
                    }
                }

                var log = new AiTrafficLog
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EndpointName = endpointName,
                    Provider = provider,
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
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
    }
}
