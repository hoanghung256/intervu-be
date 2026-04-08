using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfile
{
    public class UserProfileControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public UserProfileControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid userId, string token, string email)> RegisterAndLoginUserAsync()
        {
            var email = $"user_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Test User",
                Role = "Candidate" // Ensure it's a candidate for CV upload tests
            };

            await _api.PostAsync("/api/v1/account/register", registerRequest);

            var loginRequest = new LoginRequest { Email = email, Password = password };
            var loginResponse = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);

            return (loginData.Data!.User.Id, loginData.Data.Token, email);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task GetProfile_ReturnsSuccess_WhenUserExists()
        {
            // Arrange
            var (userId, token, _) = await RegisterAndLoginUserAsync();

            // Act
            LogInfo($"Getting profile for user {userId}.");
            var response = await _api.GetAsync($"/api/v1/userprofile/{userId}", jwtToken: token, logBody: true);

            // Assert
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
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            LogInfo($"Getting profile for non-existent user {nonExistentUserId}.");
            var response = await _api.GetAsync($"/api/v1/userprofile/{nonExistentUserId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UpdateProfile_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            var (userId, token, _) = await RegisterAndLoginUserAsync();
            var updateRequest = new UpdateProfileRequest { FullName = "Updated Test User" };

            // Act
            LogInfo($"Updating profile for user {userId}.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}", updateRequest, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<UserDto>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "Update was successful");
            await AssertHelper.AssertEqual("Updated Test User", apiResponse.Data!.FullName, "Full name was updated");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task ChangePassword_ReturnsSuccess_WhenPasswordIsCorrect()
        {
            // Arrange
            var (userId, token, _) = await RegisterAndLoginUserAsync();
            var currentPassword = CANDIDATE_PASSWORD;
            var newPassword = "NewPassword456!";
            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
            };

            // Act
            LogInfo($"Changing password for user {userId}.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}/password", changePasswordRequest, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "Password change was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task ChangePassword_ReturnsBadRequest_WhenPasswordIsIncorrect()
        {
            // Arrange
            var (userId, token, _) = await RegisterAndLoginUserAsync();
            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword!",
                NewPassword = "NewPassword456!",
            };

            // Act
            LogInfo($"Attempting to change password for user {userId} with incorrect current password.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}/password", changePasswordRequest, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task ManageAvatar_ReturnsSuccess()
        {
            // Arrange
            var (userId, token, _) = await RegisterAndLoginUserAsync();
            var fileContent = Encoding.UTF8.GetBytes("This is a dummy file.");
            var fileName = "test.png";
            var contentType = "image/png";
            var formName = "profilePicture";

            // 1. Upload Avatar
            LogInfo($"Uploading avatar for user {userId}.");
            var uploadResponse = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-avatar/{userId}", fileContent, fileName, contentType, formName, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, uploadResponse.StatusCode, "Upload status code is 200 OK");
            var uploadResult = await _api.LogDeserializeJson<AvatarUploadResponseData>(uploadResponse, true);
            await AssertHelper.AssertTrue(uploadResult.Success, "Avatar upload was successful");
            await AssertHelper.AssertNotNull(uploadResult.Data?.ProfilePictureUrl, "Profile picture URL is returned");

            // 2. Delete Avatar
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
            // Arrange
            var (userId, token, _) = await RegisterAndLoginUserAsync();

            // Act
            LogInfo($"Attempting to upload avatar for user {userId} without a file.");
            var response = await _api.PostAsync<object>($"/api/v1/userprofile/upload-avatar/{userId}", null, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UploadCV_ReturnsSuccess_WhenFileIsValid()
        {
            // Arrange
            var (userId, token, _) = await RegisterAndLoginUserAsync();
            var fileContent = Encoding.UTF8.GetBytes("This is a dummy CV file for testing.");
            var fileName = "cv.pdf";
            var contentType = "application/pdf";
            var formName = "file";

            // Act
            LogInfo($"Uploading CV for user {userId}.");
            var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-cv/{userId}", fileContent, fileName, contentType, formName, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<string>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "CV upload was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "CV URL is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task DeleteAvatar_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            LogInfo($"Attempting to delete avatar for non-existent user {nonExistentUserId}.");
            var response = await _api.DeleteAsync($"/api/v1/userprofile/delete-avatar/{nonExistentUserId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
        }

        private class AvatarUploadResponseData
        { 
            public string ProfilePictureUrl { get; set; }
        }
    }
}