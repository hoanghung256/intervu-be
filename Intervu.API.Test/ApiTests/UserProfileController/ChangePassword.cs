using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    public class ChangePasswordTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ChangePasswordTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid userId, string token)> RegisterAndLoginUserAsync()
        {
            var email = $"user_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD,
                FullName = "Test User",
                Role = "Candidate"
            });

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);

            return (loginData.Data!.User.Id, loginData.Data.Token);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task ChangePassword_ReturnsSuccess_WhenPasswordIsCorrect()
        {
            var (userId, token) = await RegisterAndLoginUserAsync();
            var changePasswordRequest = new Intervu.Application.DTOs.User.ChangePasswordRequest
            {
                CurrentPassword = CANDIDATE_PASSWORD,
                NewPassword = "NewPassword456!",
            };

            LogInfo($"Changing password for user {userId}.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}/password", changePasswordRequest, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "Password change was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task ChangePassword_ReturnsBadRequest_WhenPasswordIsIncorrect()
        {
            var (userId, token) = await RegisterAndLoginUserAsync();
            var changePasswordRequest = new Intervu.Application.DTOs.User.ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword!",
                NewPassword = "NewPassword456!",
            };

            LogInfo($"Attempting to change password for user {userId} with incorrect current password.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}/password", changePasswordRequest, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }
    }
}
