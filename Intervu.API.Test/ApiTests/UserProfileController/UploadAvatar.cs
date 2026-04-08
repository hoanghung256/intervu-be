using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    public class UploadAvatarTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public UploadAvatarTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task ManageAvatar_ReturnsSuccess()
        {
            var (userId, token) = await RegisterAndLoginUserAsync();
            var fileContent = Encoding.UTF8.GetBytes("This is a dummy file.");

            LogInfo($"Uploading avatar for user {userId}.");
            var uploadResponse = await _api.PostMultipartAsync(
                $"/api/v1/userprofile/upload-avatar/{userId}",
                fileContent,
                "test.png",
                "image/png",
                "profilePicture",
                jwtToken: token,
                logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, uploadResponse.StatusCode, "Upload status code is 200 OK");
            var uploadResult = await _api.LogDeserializeJson<AvatarUploadResponseData>(uploadResponse, true);
            await AssertHelper.AssertTrue(uploadResult.Success, "Avatar upload was successful");
            await AssertHelper.AssertNotNull(uploadResult.Data?.ProfilePictureUrl, "Profile picture URL is returned");

            LogInfo($"Deleting avatar for user {userId}.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/userprofile/delete-avatar/{userId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
            var deleteResult = await _api.LogDeserializeJson<object>(deleteResponse);
            await AssertHelper.AssertTrue(deleteResult.Success, "Avatar deletion was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UploadAvatar_ReturnsBadRequest_WhenFileIsMissing()
        {
            var (userId, token) = await RegisterAndLoginUserAsync();

            LogInfo($"Attempting to upload avatar for user {userId} without a file.");
            var response = await _api.PostAsync<object>($"/api/v1/userprofile/upload-avatar/{userId}", null, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        private class AvatarUploadResponseData
        {
            public string ProfilePictureUrl { get; set; }
        }
    }
}
