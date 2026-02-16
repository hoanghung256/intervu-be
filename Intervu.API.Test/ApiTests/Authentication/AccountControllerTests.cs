using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Authentication
{
    public class AccountControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public AccountControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }
        
        // Seeded Data
        private readonly string _aliceEmail = "alice@example.com";

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        [Trait("Category", "Smoke")]
        public async Task Register_ReturnsSuccess_WhenDataIsValid()
        {
            var registerRequest = new RegisterRequest
            {
                Email = $"test_{Guid.NewGuid()}@example.com",
                Password = ACCOUNT_PASSWORD,
                FullName = "Test_User",
            };

            LogInfo("Registering new user.");
            var response = await _api.PostAsync("/api/v1/account/register", registerRequest, logBody: true);

            LogInfo("Verify response.");
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Registration successful");
            await AssertHelper.AssertEqual("Registration successful", apiResponse.Message, "Message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        [Trait("Category", "Smoke")]
        public async Task Login_ReturnsToken_WhenCredentialsAreValid()
        {
            // Arrange
            var email = _aliceEmail;
            var password = ACCOUNT_PASSWORD;

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act
            LogInfo("Logging in.");
            var response = await _api.PostAsync("/api/v1/account/login", loginRequest, logBody: true);

            // Assert
            LogInfo("Verify response.");
            var apiResponse = await _api.LogDeserializeJson<LoginResponse>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Login successful");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Token, "Token is returned");
            
            var cookies = response.Headers.GetValues("Set-Cookie");
            await AssertHelper.AssertNotEmpty(cookies, "Cookies are set");
            await AssertHelper.AssertContains("refreshToken", cookies.First(), "RefreshToken cookie present");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Login_ReturnsFailure_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword!"
            };

            // Act
            LogInfo("Logging in with invalid credentials.");
            var response = await _api.PostAsync("/api/v1/account/login", loginRequest, logBody: true);

            // Assert
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Login should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task RefreshToken_ReturnsNewToken_WhenCookieIsValid()
        {
            // Arrange
            var email = $"refresh_{Guid.NewGuid()}@example.com";
            var password = ACCOUNT_PASSWORD;
            
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

            // Act
            LogInfo("Refreshing token.");
            var headers = new Dictionary<string, string>
            {
                { "Cookie", cookieValue! }
            };

            var response = await _api.PostAsync<object>("/api/v1/account/refresh-token", null, headers: headers, logBody: true);

            // Assert
            LogInfo("Verify response.");
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
            // Act
            LogInfo("Refreshing token without cookie.");
            var response = await _api.PostAsync<object>("/api/v1/account/refresh-token", null, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Logout_ClearsCookie_WhenAuthenticated()
        {
            // Arrange
            var email = $"logout_{Guid.NewGuid()}@example.com";
            var password = ACCOUNT_PASSWORD;
            
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

            // Act
            LogInfo("Logging out.");
            var response = await _api.PostAsync<object>("/api/v1/account/logout", null, jwtToken: token!, logBody: true);

            // Assert
            LogInfo("Verify response.");
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            
            var cookies = response.Headers.GetValues("Set-Cookie");
            var refreshTokenCookie = cookies.FirstOrDefault(c => c.Contains("refreshToken"));
            await AssertHelper.AssertContains("Expires=", refreshTokenCookie!, "Cookie expiration set to past");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            // Use seeded Alice's email which already exists
            var email = _aliceEmail;
            var request = new RegisterRequest
            {
                Email = email,
                Password = ACCOUNT_PASSWORD,
                FullName = "Duplicate User"
            };

            // Act
            LogInfo("Registering same user again.");
            var response = await _api.PostAsync("/api/v1/account/register", request, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Registration failed");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Register_ReturnsBadRequest_WhenPasswordIsWeak()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = $"weak_{Guid.NewGuid()}@example.com",
                Password = "123", // Weak password
                FullName = "Weak Password User"
            };

            // Act
            LogInfo("Registering user with weak password.");
            var response = await _api.PostAsync("/api/v1/account/register", request, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Register_ReturnsBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "invalid-email-format",
                Password = ACCOUNT_PASSWORD,
                FullName = "Invalid Email User"
            };

            // Act
            LogInfo("Registering user with invalid email.");
            var response = await _api.PostAsync("/api/v1/account/register", request, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
        }

        private class RefreshTokenResponseData
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
        }
    }
}