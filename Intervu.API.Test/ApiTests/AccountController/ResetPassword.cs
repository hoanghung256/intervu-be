using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AccountController
{
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
        public async Task Handle_InvalidToken_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/auth/reset-password", new { token = "invalid_token", newPassword = "NewPassword123!", confirmPassword = "NewPassword123!" }, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertFalse(apiResponse.Success, "Reset password should fail");
            await AssertHelper.AssertEqual("Invalid or expired token.", apiResponse.Message, "Error message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_MismatchedConfirmPassword_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/auth/reset-password", new { token = "any_token", newPassword = "NewPassword123!", confirmPassword = "DifferentPassword123!" }, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertContains("Passwords do not match.", responseBody, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ResetPassword_NewPasswordTooShort_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/auth/reset-password", new { token = "any_token", newPassword = "123", confirmPassword = "123" }, logBody: true);
            var responseBody = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertContains("Password must be at least 6 characters long.", responseBody, "Validation message matches");
        }
    }
}
