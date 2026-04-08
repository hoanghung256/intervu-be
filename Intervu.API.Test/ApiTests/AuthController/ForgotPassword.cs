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
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ForgotPassword_ReturnsBadRequest_WhenEmailIsInvalid()
        {
            var forgotRequest = new ForgotPasswordRequest { Email = "notanemail" };

            LogInfo("Requesting password reset with invalid email format.");
            var response = await _api.PostAsync("/api/v1/auth/forgot-password", forgotRequest, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
        }

        private class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }
    }
}
