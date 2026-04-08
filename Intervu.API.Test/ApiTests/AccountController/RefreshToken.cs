using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Linq;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AccountController
{
    public class RefreshTokenTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public RefreshTokenTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task RefreshToken_ReturnsNewToken_WhenCookieIsValid()
        {
            var email = $"refresh_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Refresh_User",
            });

            var loginRequest = new LoginRequest { Email = email, Password = password };
            var loginResponse = await _api.PostAsync("/api/v1/account/login", loginRequest);

            var cookies = loginResponse.Headers.GetValues("Set-Cookie");
            var refreshTokenCookie = cookies.FirstOrDefault(c => c.Contains("refreshToken"));
            var cookieValue = refreshTokenCookie?.Split(';')[0];

            LogInfo("Refreshing token.");
            var headers = new Dictionary<string, string>
            {
                { "Cookie", cookieValue! }
            };

            var response = await _api.PostAsync<object>("/api/v1/account/refresh-token", null, headers: headers, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<RefreshTokenResponseData>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Refresh successful");
            await AssertHelper.AssertNotNull(apiResponse.Data?.AccessToken, "Access token returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task RefreshToken_ReturnsUnauthorized_WhenCookieIsMissing()
        {
            LogInfo("Refreshing token without cookie.");
            var response = await _api.PostAsync<object>("/api/v1/account/refresh-token", null, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
            await AssertHelper.AssertFalse(apiResponse.Success, "Refresh should fail");
            await AssertHelper.AssertEqual("Refresh token not found", apiResponse.Message, "Error message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_RefreshToken_InvalidCookie_ReturnsUnauthorized()
        {
            var headers = new Dictionary<string, string>
            {
                { "Cookie", "refreshToken=invalid_refresh_token_value" }
            };

            var response = await _api.PostAsync<object>("/api/v1/account/refresh-token", null, headers: headers, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
            await AssertHelper.AssertFalse(apiResponse.Success, "Refresh should fail");
            await AssertHelper.AssertEqual("Invalid or expired refresh token", apiResponse.Message, "Error message matches");
        }

        private class RefreshTokenResponseData
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
        }
    }
}
