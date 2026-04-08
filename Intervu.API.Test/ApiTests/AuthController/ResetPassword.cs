using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
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
        }

        private class ResetPasswordRequest
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }
    }
}
