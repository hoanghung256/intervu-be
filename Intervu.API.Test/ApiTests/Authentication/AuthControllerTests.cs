using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Authentication
{
    public class AuthControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public AuthControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        // [Fact]
        // [Trait("Category", "API")]
        // [Trait("Category", "Authentication")]
        // public async Task GoogleLogin_ReturnsBadRequest_WhenTokenIsInvalid()
        // {
        //     // Arrange
        //     var request = new 
        //     { 
        //         idToken = "invalid_fake_token_string_12345" 
        //     };
        //
        //     // Act
        //     LogInfo("Sending invalid google token.");
        //     var response = await _api.PostAsync("/api/v1/auth/google", request, logBody: true);
        //
        //     // Assert
        //     await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
        //     var apiResponse = await _api.LogDeserializeJson<object>(response);
        //     await AssertHelper.AssertFalse(apiResponse.Success, "Google login failed");
        // }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ForgotPassword_ReturnsSuccess_WhenEmailExists()
        {
            // Arrange: Create a user first
            var email = $"forgot_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = "Password123!",
                FullName = "Forgot Password User"
            });

            var forgotRequest = new ForgotPasswordRequest { Email = email };

            // Act
            LogInfo($"Requesting password reset for {email}.");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Forgot password request successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ForgotPassword_ReturnsBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            var forgotRequest = new ForgotPasswordRequest { Email = "notanemail" };

            // Act
            LogInfo("Requesting password reset with invalid email format.");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ValidateResetToken_ReturnsBadRequest_WhenTokenIsInvalid()
        {
            // Act
            LogInfo("Validating a fake reset token.");
            var response = await _api.GetAsync("/api/v1/auth/validate-reset-token/fake-token-123", logBody: true);

            // Assert
            // Note: Depending on implementation, invalid token might return 400 or 200 with success=false. 
            // Based on Controller code: return BadRequest(new { success = false... })
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Validation failed");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ResetPassword_ReturnsBadRequest_WhenTokenIsInvalid()
        {
            // Arrange
            var resetRequest = new ResetPasswordRequest
            {
                Token = "invalid_token",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            LogInfo("Resetting password with invalid token.");
            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Reset password failed");
        }

        // Private DTOs to ensure tests are self-contained
        private class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }

        private class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }
    }
}