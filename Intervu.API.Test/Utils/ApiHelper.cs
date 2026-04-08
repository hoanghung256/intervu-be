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
            _test = BaseTest.Current.Value!;
        }

        private async Task<T> LogApiActionAsync<T>(string stepName, Func<Task<T>> action)
        {
            if (_test != null)
            {
                return await _test.LogStepAsync(stepName, action);
            }
            return await action();
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, string jwtToken = "", bool logBody = false)
        {
            return await LogApiActionAsync($"GET {requestUri}", async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                if (!string.IsNullOrEmpty(jwtToken))
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
                if (!string.IsNullOrEmpty(jwtToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                if (headers != null)
                {
                    foreach (var header in headers)
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                if (payload != null)
                {
                    var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                    _test.LogInfo($"Request Body:<br/><pre>{jsonPayload}</pre>");
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
                if (!string.IsNullOrEmpty(jwtToken))
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
                if (!string.IsNullOrEmpty(jwtToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                _test.LogInfo($"Request Body:<br/><pre>{jsonPayload}</pre>");
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
                if (!string.IsNullOrEmpty(jwtToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await _client.SendAsync(request);
                await LogResponseDetails(response, logBody);
                return response;
            });
        }

        public async Task<HttpResponseMessage> PatchAsync<T>(string requestUri, T payload, string jwtToken = "", bool logBody = false)
        {
            return await LogApiActionAsync($"PATCH {requestUri}", async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, requestUri);
                if (!string.IsNullOrEmpty(jwtToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                _test.LogInfo($"Request Body:<br/><pre>{jsonPayload}</pre>");
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _client.SendAsync(request);
                await LogResponseDetails(response, logBody);
                return response;
            });
        }

        private async Task LogResponseDetails(HttpResponseMessage response, bool logBody = false)
        {
            _test.LogInfo($"Response: <b>{(int)response.StatusCode} {response.ReasonPhrase}</b>");

            if (logBody)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    _test.LogInfo($"Response Body:<br/><pre>{TruncateLog(responseBody)}</pre>");
                }
            }
        }

        public async Task<ApiResponse<T>> LogDeserializeJson<T>(HttpResponseMessage response, bool logBody = false)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var json = TestUtils.DeserializeJson<ApiResponse<T>>(content);
                if (json == null) throw new Exception("Deserialization returned null.");

                if (logBody && !string.IsNullOrWhiteSpace(content))
                {
                    _test.LogInfo($"Deserialized Response Body:<br/><pre>{TruncateLog(content)}</pre>");
                }

                return json;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Failed to deserialize response to {typeof(ApiResponse<T>).Name}.<br/>Error: {ex.Message}<br/>Response Body: <pre>{content}</pre>";
                await _test.LogFail(errorMsg);
                throw new Exception(errorMsg, ex);
            }
        }

        private string TruncateLog(string content)
        {
            const int MaxLength = 10000;
            if (string.IsNullOrEmpty(content) || content.Length <= MaxLength) return content;
            return content.Substring(0, MaxLength) + $"\n... [Truncated {content.Length - MaxLength} chars]";
        }

        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }
    }
}
