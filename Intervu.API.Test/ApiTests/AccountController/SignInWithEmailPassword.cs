using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Linq;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AccountController
{
    public class SignInWithEmailPasswordTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SignInWithEmailPasswordTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        [Trait("Category", "Smoke")]
        public async Task Login_ReturnsToken_WhenCredentialsAreValid()
        {
            var email = $"login_test_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;

            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Login Test User"
            };

            LogInfo($"Registering new user: {email}");
            var registerResponse = await _api.PostAsync("/api/v1/account/register", registerRequest, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, registerResponse.StatusCode, "Registration should succeed");

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            LogInfo("Logging in with the newly registered account.");
            var response = await _api.PostAsync("/api/v1/account/login", loginRequest, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<LoginResponse>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Login successful");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Token, "Token is returned");
            await AssertHelper.AssertEqual(email, apiResponse.Data?.User?.Email, "User email matches");

            var cookies = response.Headers.GetValues("Set-Cookie");
            await AssertHelper.AssertNotEmpty(cookies, "Cookies are set");
            await AssertHelper.AssertContains("refreshToken", cookies.First(), "RefreshToken cookie present");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Login_ReturnsFailure_WhenCredentialsAreInvalid()
        {
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword!"
            };

            LogInfo("Logging in with invalid credentials.");
            var response = await _api.PostAsync("/api/v1/account/login", loginRequest, logBody: true);

            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for invalid credentials");
            await AssertHelper.AssertFalse(apiResponse.Success, "Login should fail");
            await AssertHelper.AssertEqual("Invalid email or password", apiResponse.Message, "Error message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_Login_MissingEmail_ReturnsFailureResponse()
        {
            var request = new LoginRequest
            {
                Email = "",
                Password = CANDIDATE_PASSWORD
            };

            var response = await _api.PostAsync("/api/v1/account/login", request, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertFalse(apiResponse.Success, "Login should fail");
            await AssertHelper.AssertEqual("Invalid email or password", apiResponse.Message, "Error message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_Login_MissingPassword_ReturnsFailureResponse()
        {
            var request = new LoginRequest
            {
                Email = $"missingpass_{Guid.NewGuid()}@example.com",
                Password = ""
            };

            var response = await _api.PostAsync("/api/v1/account/login", request, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertFalse(apiResponse.Success, "Login should fail");
            await AssertHelper.AssertEqual("Invalid email or password", apiResponse.Message, "Error message matches");
        }
    }
}
