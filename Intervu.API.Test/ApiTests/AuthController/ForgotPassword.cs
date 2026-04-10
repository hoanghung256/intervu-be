using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    public class ForgotPasswordTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ForgotPasswordTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ForgotPassword_ReturnsSuccess_WhenEmailExists()
        {
            var email = $"forgot_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = "Password123!",
                FullName = "Forgot Password User"
            });

            var forgotRequest = new ForgotPasswordRequest { Email = email };

            LogInfo($"Requesting password reset for {email}.");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Forgot password request successful");
            await AssertHelper.AssertEqual("Password reset link has been sent to your email.", apiResponse.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ForgotPassword_ReturnsBadRequest_WhenEmailIsInvalid()
        {
            var forgotRequest = new ForgotPasswordRequest { Email = "notanemail" };

            LogInfo("Requesting password reset with invalid email format.");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertContains("Invalid email address format.", responseBody, "Validation message is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_UnknownEmail_ReturnsGenericSuccess()
        {
            var forgotRequest = new ForgotPasswordRequest { Email = $"unknown_{Guid.NewGuid()}@example.com" };

            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Unknown email should still return success for security reasons");
            await AssertHelper.AssertEqual("If the email is registered, a password reset link has been sent.", apiResponse.Message, "Generic success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_EmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var forgotRequest = new ForgotPasswordRequest { Email = "" };

            // Act
            LogInfo("Requesting password reset with empty email string.");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertNotNull(responseBody, "Error response body is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_NullEmail_ReturnsBadRequest()
        {
            // Arrange
            var forgotRequest = new ForgotPasswordRequest { Email = null! };

            // Act
            LogInfo("Requesting password reset with null email value.");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest for null email");
            await AssertHelper.AssertNotNull(responseBody, "Error response body is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_VeryLongEmail_ReturnsBadRequest()
        {
            // Arrange – RFC 5321 limits local-part to 64 chars; a 300-char local part is invalid
            var longLocalPart = new string('a', 300);
            var forgotRequest = new ForgotPasswordRequest { Email = $"{longLocalPart}@example.com" };

            // Act
            LogInfo("Requesting password reset with over-length email local part (boundary).");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert – email validation rejects the over-limit address
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Over-length email returns 400 BadRequest");
            await AssertHelper.AssertNotNull(responseBody, "Validation error body is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_EmailWithLeadingWhitespace_ReturnsBadRequest()
        {
            // Arrange – leading space makes the string an invalid email address
            var forgotRequest = new ForgotPasswordRequest { Email = " alice@example.com" };

            // Act
            LogInfo("Requesting password reset with leading-whitespace email.");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Email with leading whitespace returns 400 BadRequest");
            await AssertHelper.AssertNotNull(responseBody, "Validation error body is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_CalledTwiceForSameEmail_BothReturnSuccess()
        {
            // Arrange – register a fresh account so the email definitely exists
            var email = $"double_forgot_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = "Password123!",
                FullName = "Double Forgot User"
            });
            var request = new ForgotPasswordRequest { Email = email };

            // Act – first call
            var first = await _api.PostAsync("/api/v1/auth/forgot-password", request, logBody: true);
            // Act – second call (idempotency)
            var second = await _api.PostAsync("/api/v1/auth/forgot-password", request, logBody: true);

            // Assert – both invocations succeed
            await AssertHelper.AssertEqual(HttpStatusCode.OK, first.StatusCode, "First forgot-password call returns 200 OK");
            await AssertHelper.AssertEqual(HttpStatusCode.OK, second.StatusCode, "Second forgot-password call also returns 200 OK (idempotent)");
            var firstPayload = await _api.LogDeserializeJson<object>(first);
            var secondPayload = await _api.LogDeserializeJson<object>(second);
            await AssertHelper.AssertTrue(firstPayload.Success, "First call succeeds");
            await AssertHelper.AssertTrue(secondPayload.Success, "Second call succeeds (idempotent behaviour)");
        }
        
        private class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }

        // ===== Tests moved from AccountController/ForgotPassword.cs =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ValidEmail_ReturnsSuccess_FromAccountController()
        {
            var email = $"forgot_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Forgot User" });

            var response = await _api.PostAsync("/api/v1/auth/forgot-password", new { email }, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Forgot password should succeed");
            await AssertHelper.AssertEqual("Password reset link has been sent to your email.", apiResponse.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_InvalidEmailFormat_ReturnsBadRequest_FromAccountController()
        {
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", new { email = "invalid-email" }, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertContains("Invalid email address format.", responseBody, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_UnknownEmail_ReturnsGenericSuccess_FromAccountController()
        {
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", new { email = $"unknown_{Guid.NewGuid()}@example.com" }, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Forgot password should still succeed for unknown email");
            await AssertHelper.AssertEqual("If the email is registered, a password reset link has been sent.", apiResponse.Message, "Generic security message matches");
        }
    }
}

// --- Tests moved from AccountController/ForgotPassword.cs ---
// These were consolidated here to keep auth-related tests together.
// Original file replaced with a placeholder.
