using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Intervu.API.Test.Base;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Skill;

namespace Intervu.API.Test.Utils
{
    /// <summary>
    /// A helper class that wraps HttpClient to provide automatic step logging for API requests.
    /// </summary>
    public class ApiHelper
    {
        private readonly HttpClient _client;
        private readonly BaseTest _test;

        public ApiHelper(HttpClient client)
        {
            _client = client;
            // Get the current test instance for logging purposes, similar to BaseControl
            _test = BaseTest.Current.Value!;
        }

        private async Task<T> LogApiActionAsync<T>(string stepName, Func<Task<T>> action)
        {
            if (_test != null)
            {
                return await _test.LogStepAsync(stepName, action);
            }
            else
            {
                // Fallback if not run in a test context
                return await action();
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, string jwtToken = "", bool logBody = false)
        {
            return await LogApiActionAsync($"GET {requestUri}", async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _client.SendAsync(request);
                await LogResponseDetails(response, logBody);
                return response;
            });
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string requestUri, T? payload, string jwtToken = "", Dictionary<string, string>? headers = null, bool logBody = false)
        {
            return await LogApiActionAsync($"POST {requestUri}", async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                if (payload != null)
                {
                    var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                    _test.LogInfo($"Request Body:\n{jsonPayload}");
                    request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                }
                
                var response = await _client.SendAsync(request);
                await LogResponseDetails(response, logBody);
                return response;
            });
        }

        public async Task<HttpResponseMessage> PostMultipartAsync(string requestUri, byte[] fileContent, string fileName, string contentType, string formName, string jwtToken = "", bool logBody = false)
        {
            return await LogApiActionAsync($"POST (Multipart) {requestUri}", async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var multipartContent = new MultipartFormDataContent();

                var byteArrayContent = new ByteArrayContent(fileContent);
                byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                multipartContent.Add(byteArrayContent, formName, fileName);

                request.Content = multipartContent;

                var response = await _client.SendAsync(request);
                await LogResponseDetails(response, logBody);
                return response;
            });
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string requestUri, T payload, string jwtToken = "", bool logBody = false)
        {
            return await LogApiActionAsync($"PUT {requestUri}", async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                _test.LogInfo($"Request Body:\n{jsonPayload}");
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                var response = await _client.SendAsync(request);
                await LogResponseDetails(response, logBody);
                return response;
            });
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, string jwtToken = "", bool logBody = false)
        {
            return await LogApiActionAsync($"DELETE {requestUri}", async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var response = await _client.SendAsync(request);
                await LogResponseDetails(response, logBody);
                return response;
            });
        }

        private async Task LogResponseDetails(HttpResponseMessage response, bool logBody = false)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _test.LogInfo($"Received {response.RequestMessage?.Method} Response: {(int)response.StatusCode} {response.ReasonPhrase}");
            
            if (!string.IsNullOrWhiteSpace(responseBody) && logBody)
            {
                _test.LogInfo($"\nResponse Body:\n{responseBody}");
            }
        }

        public async Task<ApiResponse<T>> LogDeserializeJson<T>(HttpResponseMessage response, bool logBody = false)
        {
            var content = await response.Content.ReadAsStringAsync();
            _test.LogInfo($"Deserializing content to {typeof(ApiResponse<T>).Name} ...");
            try
            {
                var json = TestUtils.DeserializeJson<ApiResponse<T>>(content);
                if (json == null) throw new Exception("Deserialization returned null.");
                await _test.LogPass($"Deserializing content to {typeof(ApiResponse<T>).Name} successful");
                if (logBody && !string.IsNullOrWhiteSpace(content))
                {
                    _test.LogInfo($"\nResponse Body:\n{content}");
                }
                return json;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Failed to deserialize response to {typeof(ApiResponse<T>).Name}.\nError: {ex.Message}\nResponse Body: {content}";
                await _test.LogFail(errorMsg);
                throw new Exception(errorMsg, ex);
            }
        }
            
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }
}