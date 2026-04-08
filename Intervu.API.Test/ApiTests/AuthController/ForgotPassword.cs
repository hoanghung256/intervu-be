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

        private class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }
    }
}
