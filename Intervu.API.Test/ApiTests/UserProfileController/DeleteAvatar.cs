using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    public class DeleteAvatarTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public DeleteAvatarTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid userId, string token)> RegisterAndLoginUserAsync()
        {
            var email = $"avatar_user_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD,
                FullName = "Avatar Tester",
                Role = "Candidate"
            });

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);

            return (loginData.Data!.User.Id, loginData.Data.Token);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task DeleteAvatar_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var nonExistentUserId = Guid.NewGuid();

            LogInfo($"Attempting to delete avatar for non-existent user {nonExistentUserId}.");
            var response = await _api.DeleteAsync($"/api/v1/userprofile/delete-avatar/{nonExistentUserId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task DeleteAvatar_ReturnsSuccess_WhenAvatarExists()
        {
            var (userId, token) = await RegisterAndLoginUserAsync();
            var avatarContent = Encoding.UTF8.GetBytes("fake-avatar-content");

            var uploadResponse = await _api.PostMultipartAsync(
                $"/api/v1/userprofile/upload-avatar/{userId}",
                avatarContent,
                "avatar.png",
                "image/png",
                "profilePicture",
                jwtToken: token,
                logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, uploadResponse.StatusCode, "Avatar upload status code is 200 OK");

            var deleteResponse = await _api.DeleteAsync($"/api/v1/userprofile/delete-avatar/{userId}", jwtToken: token, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(deleteResponse, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Avatar deletion succeeds");
            await AssertHelper.AssertEqual("Avatar deleted successfully", apiResponse.Message, "Delete message matches");
        }
    }
}
