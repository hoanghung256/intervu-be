using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Linq;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AccountController
{
    public class SignOutTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SignOutTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Logout_ClearsCookie_WhenAuthenticated()
        {
            var email = $"logout_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Logout_User"
            });

            var loginRequest = new LoginRequest { Email = email, Password = password };
            var loginResponse = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var token = loginData.Data?.Token;

            LogInfo("Logging out.");
            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: token!, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");

            var cookies = response.Headers.GetValues("Set-Cookie");
            var refreshTokenCookie = cookies.FirstOrDefault(c => c.Contains("refreshToken"));
            await AssertHelper.AssertContains("Expires=", refreshTokenCookie!, "Cookie expiration set to past");
        }
    }
}
