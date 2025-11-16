using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.ExternalServices
{
    public class ApiRequest
    {
        public Properties properties { get; set; }
    }

    public class ApiResponse
    {
        public string stdout { get; set; }
        public string stderr { get; set; }
        public string exception { get; set; }
        public int executionTime { get; set; }
    }

    public class Properties
    {
        public string language { get; set; }
        public List<FileItem> files { get; set; }
    }

    public class FileItem
    {
        public string name { get; set; }
        public string content { get; set; }
    }

    public class CodeExecutionService
    {
        private readonly HttpClient _httpClient;

        public CodeExecutionService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("CodeExecutionClient");
        }

        private static readonly Dictionary<string, string> FileNameMap = new()
        {
            { "javascript", "index.js" },
            { "csharp", "HelloWorld.cs" },
            { "python", "main.py" },
            { "java", "Main.java" },
            { "c", "Main.c" },
            { "cpp", "Main.cpp" },
            { "lua", "Main.lua" },
        };

        // Main method to send JSON request
        public async Task<ApiResponse> SendRequestAsync(string code, string language)
        {
            language = language?.ToLower();

            // 🔥 Throw when language is not in dictionary
            if (!FileNameMap.ContainsKey(language))
            {
                throw new ArgumentException(
                    $"Language '{language}' is not supported. " +
                    $"Supported languages: {string.Join(", ", FileNameMap.Keys)}"
                );
            }

            var fileName = FileNameMap[language];
            // Build request object
            var request = new ApiRequest
            {
                properties = new Properties
                {
                    language = language,
                    files = new List<FileItem>
                {
                    new FileItem
                    {
                        name = fileName,
                        content = code
                    }
                }
                }
            };

            string json = JsonSerializer.Serialize(
                request,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("exec", content);

            response.EnsureSuccessStatusCode();
            // Deserialize the JSON response into ApiResponse
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

            if (!string.IsNullOrEmpty(apiResponse.stderr))
            {
                apiResponse.stderr = apiResponse.stderr.Replace(fileName, "");
            }

            if (!string.IsNullOrEmpty(apiResponse.exception))
            {
                apiResponse.exception = apiResponse.exception.Replace(fileName, "");
            }

            // Return the parsed ApiResponse object
            return apiResponse;
        }
    }
}
