using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    public class ViewPersonalInformationTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewPersonalInformationTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid userId, string token)> RegisterAndLoginUserAsync()
        {
            var email = $"user_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Test User",
                Role = "Candidate"
            };

            await _api.PostAsync("/api/v1/account/register", registerRequest);

            var loginRequest = new LoginRequest { Email = email, Password = password };
            var loginResponse = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);

            return (loginData.Data!.User.Id, loginData.Data.Token);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task GetProfile_ReturnsSuccess_WhenUserExists()
        {
            var (userId, token) = await RegisterAndLoginUserAsync();

            LogInfo($"Getting profile for user {userId}.");
            var response = await _api.GetAsync($"/api/v1/userprofile/{userId}", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<UserDto>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertEqual(userId, apiResponse.Data!.Id, "User ID matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task GetProfile_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var nonExistentUserId = Guid.NewGuid();

            LogInfo($"Getting profile for non-existent user {nonExistentUserId}.");
            var response = await _api.GetAsync($"/api/v1/userprofile/{nonExistentUserId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
        }
    }
}
