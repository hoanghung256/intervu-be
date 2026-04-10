using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Linq;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    // IC-6
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
        public async Task Handle_AuthenticatedUser_SignsOutSuccessfully()
        {
            var email = $"logout_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Logout User" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: loginData.Data!.Token, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Logout should succeed");
            await AssertHelper.AssertEqual("Logged out successfully", apiResponse.Message, "Success message matches");

            var cookies = response.Headers.GetValues("Set-Cookie");
            await AssertHelper.AssertContains("Expires=", cookies.First(), "Refresh token cookie is expired");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_SignOut_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_SignOut_WithMalformedJwt_ReturnsUnauthorized()
        {
            // Arrange – a syntactically invalid JWT (not three dot-separated base64 segments)
            const string malformedToken = "this.is.not.a.valid.jwt.at.all";

            // Act
            LogInfo("Attempting logout with a malformed JWT token.");
            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: malformedToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Malformed JWT returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_SignOut_WithRandomStringToken_ReturnsUnauthorized()
        {
            // Arrange – a random alphanumeric string with no JWT structure
            const string junkToken = "abc123notareatoken";

            // Act
            LogInfo("Attempting logout with a random non-JWT string.");
            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: junkToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Non-JWT token string returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_SignOut_WithInvalidToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: "invalid-token", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Invalid token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_SignOut_Twice_ReturnsUnauthorized()
        {
            var email = $"logout_twice_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = DEFAULT_PASSWORD, FullName = "Logout User" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // First logout
            await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: loginData.Data!.Token);

            // Second logout with same token
            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Second logout with same token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_SignOut_WithEmptyToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: "", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Empty token returns 401 Unauthorized");
        }
    }
}
