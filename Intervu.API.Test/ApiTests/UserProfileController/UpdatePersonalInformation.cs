using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    public class UpdatePersonalInformationTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public UpdatePersonalInformationTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid userId, string token)> RegisterAndLoginUserAsync()
        {
            var email = $"user_{Guid.NewGuid()}@example.com";
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD,
                FullName = "Test User",
                Role = "Candidate"
            };

            await _api.PostAsync("/api/v1/account/register", registerRequest);
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);

            return (loginData.Data!.User.Id, loginData.Data.Token);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UpdateProfile_ReturnsSuccess_WhenDataIsValid()
        {
            var (userId, token) = await RegisterAndLoginUserAsync();
            var updateRequest = new UpdateProfileRequest { FullName = "Updated Test User" };

            LogInfo($"Updating profile for user {userId}.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}", updateRequest, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<UserDto>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "Update was successful");
            await AssertHelper.AssertEqual("Updated Test User", apiResponse.Data!.FullName, "Full name was updated");
        }
    }
}
