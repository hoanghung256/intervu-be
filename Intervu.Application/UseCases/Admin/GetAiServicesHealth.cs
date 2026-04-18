using Intervu.Application.DTOs.Admin;
using Intervu.Application.Interfaces.UseCases.Admin;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAiServicesHealth : IGetAiServicesHealth
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GetAiServicesHealth(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<ServiceHealthDto>> ExecuteAsync()
        {
            var results = new List<ServiceHealthDto>();
            var now = DateTime.UtcNow;

            // 1. Python AI Service — active ping
            var pythonBaseUrl = _configuration["PythonAiService:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
            results.Add(await PingHttpAsync("Python AI Service", pythonBaseUrl, "/health", now));

            // 2. Pinecone — active ping via describe_index_stats
            var pineconeHost = _configuration["PineCone:PINECONE_HOST_URL"] ?? string.Empty;
            var pineconeKey = _configuration["PineCone:PINECONE_API_KEY"] ?? string.Empty;
            results.Add(await PingPineconeAsync(pineconeHost, pineconeKey, now));

            // 3. HuggingFace Router — config check only (auth required for actual call)
            var hfBaseUrl = _configuration["ReasoningApi:BaseUrl"] ?? string.Empty;
            var hfKey = _configuration["ReasoningApi:AI_API_KEY"] ?? string.Empty;
            results.Add(BuildConfigDto("HuggingFace Router", hfBaseUrl, hfKey, now));

            // 4. Gemini API — config check only
            var geminiKey = _configuration["GeminiApi:GEMINI_API_KEY"] ?? string.Empty;
            var geminiModel = _configuration["GeminiApi:ModelId"] ?? string.Empty;
            var geminiEndpoint = $"https://generativelanguage.googleapis.com (model: {geminiModel})";
            results.Add(BuildConfigDto("Gemini API", geminiEndpoint, geminiKey, now));

            return results;
        }

        private async Task<ServiceHealthDto> PingHttpAsync(string name, string baseUrl, string path, DateTime checkedAt)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return new ServiceHealthDto
                {
                    ServiceName = name,
                    Status = "KeyMissing",
                    Endpoint = "(not configured)",
                    CheckedAt = checkedAt
                };
            }

            var sw = Stopwatch.StartNew();
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync($"{baseUrl}{path}");
                sw.Stop();

                return new ServiceHealthDto
                {
                    ServiceName = name,
                    Endpoint = $"{baseUrl}{path}",
                    Status = response.IsSuccessStatusCode ? "Healthy" : "Degraded",
                    ResponseTimeMs = sw.ElapsedMilliseconds,
                    CheckedAt = checkedAt
                };
            }
            catch
            {
                sw.Stop();
                return new ServiceHealthDto
                {
                    ServiceName = name,
                    Endpoint = $"{baseUrl}{path}",
                    Status = "Unreachable",
                    ResponseTimeMs = sw.ElapsedMilliseconds,
                    CheckedAt = checkedAt
                };
            }
        }

        private async Task<ServiceHealthDto> PingPineconeAsync(string hostUrl, string apiKey, DateTime checkedAt)
        {
            if (string.IsNullOrWhiteSpace(hostUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                return new ServiceHealthDto
                {
                    ServiceName = "Pinecone",
                    Status = "KeyMissing",
                    Endpoint = "(not configured)",
                    CheckedAt = checkedAt
                };
            }

            var endpoint = $"{hostUrl.TrimEnd('/')}/describe_index_stats";
            var sw = Stopwatch.StartNew();
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(8);
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Api-Key", apiKey);
                request.Headers.Add("X-Pinecone-Api-Version", _configuration["PineCone:PINECONE_API_VERSION"] ?? "2025-10");

                var response = await client.SendAsync(request);
                sw.Stop();

                return new ServiceHealthDto
                {
                    ServiceName = "Pinecone",
                    Endpoint = hostUrl,
                    Status = response.IsSuccessStatusCode ? "Healthy" : "Degraded",
                    ResponseTimeMs = sw.ElapsedMilliseconds,
                    CheckedAt = checkedAt
                };
            }
            catch
            {
                sw.Stop();
                return new ServiceHealthDto
                {
                    ServiceName = "Pinecone",
                    Endpoint = hostUrl,
                    Status = "Unreachable",
                    ResponseTimeMs = sw.ElapsedMilliseconds,
                    CheckedAt = checkedAt
                };
            }
        }

        private static ServiceHealthDto BuildConfigDto(string name, string endpoint, string key, DateTime checkedAt) =>
            new ServiceHealthDto
            {
                ServiceName = name,
                Endpoint = endpoint,
                Status = string.IsNullOrWhiteSpace(key) ? "KeyMissing" : "Configured",
                ResponseTimeMs = 0,
                CheckedAt = checkedAt
            };
    }
}
