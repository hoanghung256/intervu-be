using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    // IC-4
    public class ResetPasswordTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ResetPasswordTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ResetPassword_ReturnsBadRequest_WhenTokenIsInvalid()
        {
            var resetRequest = new ResetPasswordRequest
            {
                Token = "invalid_token",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            LogInfo("Resetting password with invalid token.");
            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Reset password failed");
            await AssertHelper.AssertEqual("Invalid or expired token.", apiResponse.Message, "Error message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_MismatchedConfirmPassword_ReturnsBadRequest()
        {
            var resetRequest = new ResetPasswordRequest
            {
                Token = "any_token",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "MismatchPassword123!"
            };

            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertContains("Passwords do not match.", responseBody, "Validation message is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_NewPasswordTooShort_ReturnsBadRequest()
        {
            var resetRequest = new ResetPasswordRequest
            {
                Token = "any_token",
                NewPassword = "123",
                ConfirmPassword = "123"
            };

            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertContains("Password must be at least 6 characters long.", responseBody, "Validation message is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_EmptyToken_ReturnsBadRequest()
        {
            // Arrange
            var resetRequest = new ResetPasswordRequest
            {
                Token = "",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            LogInfo("Resetting password with empty token string.");
            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest for empty token");
            await AssertHelper.AssertNotNull(responseBody, "Error response body is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_EmptyNewPassword_ReturnsBadRequest()
        {
            // Arrange
            var resetRequest = new ResetPasswordRequest
            {
                Token = "some_token",
                NewPassword = "",
                ConfirmPassword = ""
            };

            // Act
            LogInfo("Resetting password with empty new password value.");
            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest for empty password");
            await AssertHelper.AssertNotNull(responseBody, "Validation error response is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_FiveCharPassword_BelowMinLength_ReturnsBadRequest()
        {
            // Arrange – 5 chars is one below the minimum boundary of 6
            var resetRequest = new ResetPasswordRequest
            {
                Token = "any_token",
                NewPassword = "Ab1!x",
                ConfirmPassword = "Ab1!x"
            };

            // Act
            LogInfo("Resetting password with a 5-char password (boundary: min length is 6).");
            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert – fails MinLength(6) validation
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "5-char password below min boundary returns 400 BadRequest");
            await AssertHelper.AssertContains("Password must be at least 6 characters long.", responseBody, "Min-length validation message is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_NullToken_ReturnsBadRequest()
        {
            // Arrange – null token should fail [Required] validation
            var resetRequest = new ResetPasswordRequest
            {
                Token = null!,
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            LogInfo("Resetting password with null token.");
            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Null token returns 400 BadRequest");
            await AssertHelper.AssertNotNull(responseBody, "Error response body is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_NullNewPassword_ReturnsBadRequest()
        {
            // Arrange – null passwords should fail [Required] validation
            var resetRequest = new ResetPasswordRequest
            {
                Token = "some_token",
                NewPassword = null!,
                ConfirmPassword = null!
            };

            // Act
            LogInfo("Resetting password with null new password.");
            var response = await _api.PostAsync("/api/v1/auth/reset-password", resetRequest, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Null new password returns 400 BadRequest");
            await AssertHelper.AssertNotNull(responseBody, "Error response body is returned");
        }

        private class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }
    }
}
