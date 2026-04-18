using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class GetPythonAiMetricsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public GetPythonAiMetricsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Theory]
        [InlineData("24h")]
        [InlineData("7d")]
        [InlineData("30d")]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetPythonAiMetrics_WithValidTimeframe_ReturnsSuccess(string timeframe)
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync($"/api/v1/admin/system/python-ai-metrics?timeframe={timeframe}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, $"Status code is 200 OK for timeframe '{timeframe}'");
            var apiResponse = await _api.LogDeserializeJson<PythonAiMetricsDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Metrics payload is not null");
            await AssertHelper.AssertNotNull(apiResponse.Data!.Logs, "Logs collection is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetPythonAiMetrics_AggregatesMatchLogRows()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/system/python-ai-metrics?timeframe=30d", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PythonAiMetricsDto>(response);
            var data = apiResponse.Data!;

            // TotalRequests must equal the number of returned log rows
            await AssertHelper.AssertEqual(data.Logs.Count, data.TotalRequests, "TotalRequests equals Logs.Count");

            // TotalTokens must equal the sum of prompt + completion tokens
            var expectedTotalTokens = data.TotalPromptTokens + data.TotalCompletionTokens;
            await AssertHelper.AssertEqual(expectedTotalTokens, data.TotalTokens, "TotalTokens equals prompt + completion tokens");

            // Per-row TotalTokens must equal the row's prompt + completion
            foreach (var row in data.Logs)
            {
                var rowExpected = row.PromptTokens + row.CompletionTokens;
                await AssertHelper.AssertEqual(rowExpected, row.TotalTokens, $"Row {row.Id}: TotalTokens equals prompt + completion");
            }

            // Average latency: when there are logs, must be >= 0; when empty, must be 0
            if (data.Logs.Count == 0)
            {
                await AssertHelper.AssertEqual(0d, data.AverageLatencyMs, "AverageLatencyMs is 0 when no logs");
            }
            else
            {
                await AssertHelper.AssertTrue(data.AverageLatencyMs >= 0, "AverageLatencyMs is non-negative when logs exist");
            }
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetPythonAiMetrics_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/admin/system/python-ai-metrics?timeframe=24h", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No auth token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetPythonAiMetrics_WithoutTimeframeParam_DefaultsTo24h()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/system/python-ai-metrics", jwtToken: token, logBody: true);

            // Assert: default timeframe binding should give a 200 OK (not a 400)
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Missing timeframe falls back to default and returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PythonAiMetricsDto>(response);
            await AssertHelper.AssertNotNull(apiResponse.Data, "Default timeframe still returns metrics payload");
        }
    }
}
