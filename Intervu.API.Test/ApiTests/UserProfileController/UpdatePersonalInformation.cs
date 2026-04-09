using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    // IC-8
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

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UpdateProfile_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var (_, token) = await RegisterAndLoginUserAsync();
            var nonExistentUserId = Guid.NewGuid();
            var updateRequest = new UpdateProfileRequest { FullName = "Updated Test User" };

            var response = await _api.PutAsync($"/api/v1/userprofile/{nonExistentUserId}", updateRequest, jwtToken: token, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
            await AssertHelper.AssertFalse(apiResponse.Success, "Update should fail for unknown user");
            await AssertHelper.AssertEqual("User not found", apiResponse.Message, "Error message matches");
        }

        // --- Avatar tests moved here ---

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

        private class AvatarUploadResponseData
        {
            public string ProfilePictureUrl { get; set; }
        }
    }
}
