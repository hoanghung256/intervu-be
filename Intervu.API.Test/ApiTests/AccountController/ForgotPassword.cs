using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AccountController
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
        public async Task Handle_ValidEmail_ReturnsSuccess()
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
        public async Task Handle_ForgotPassword_InvalidEmailFormat_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", new { email = "invalid-email" }, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertContains("Invalid email address format.", responseBody, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ForgotPassword_UnknownEmail_ReturnsGenericSuccess()
        {
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", new { email = $"unknown_{Guid.NewGuid()}@example.com" }, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Forgot password should still succeed for unknown email");
            await AssertHelper.AssertEqual("If the email is registered, a password reset link has been sent.", apiResponse.Message, "Generic security message matches");
        }
    }
}
