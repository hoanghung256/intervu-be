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
        
        // Seeded Data
        private readonly Guid _aliceId = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
        private readonly string _aliceEmail = "alice@example.com";

        private async Task<(Guid userId, string password, string email)> CreateTestUserAsync()
        {
            var email = $"user_{Guid.NewGuid()}@example.com";
            var password = ACCOUNT_PASSWORD;
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Test User"
            };

            await _api.PostAsync("/api/v1/account/register", registerRequest);

            var loginRequest = new LoginRequest { Email = email, Password = password };
            var loginResponse = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            return (loginData.Data!.User.Id, password, email);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task GetProfile_ReturnsSuccess_WhenUserExists()
        {
            // Arrange
            // Use seeded Alice
            var userId = _aliceId;
            var email = _aliceEmail;

            // Act
            LogInfo($"Getting profile for user {userId}.");
            var response = await _api.GetAsync($"/api/v1/userprofile/{userId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<UserDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertEqual(userId, apiResponse.Data!.Id, "User ID matches");
            await AssertHelper.AssertEqual(email, apiResponse.Data!.Email, "User email matches");
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
            var userId = _aliceId;
            var updateRequest = new UpdateProfileRequest { FullName = "Updated Test User" };

            // Act
            LogInfo($"Updating profile for user {userId}.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}", updateRequest, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<UserDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Update was successful");
            await AssertHelper.AssertEqual("Updated Test User", apiResponse.Data!.FullName, "Full name was updated");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task ChangePassword_ReturnsSuccess_WhenPasswordIsCorrect()
        {
            // Arrange
            // Create a NEW user for this test to avoid breaking seeded user credentials
            var (userId, currentPassword, _) = await CreateTestUserAsync();
            var newPassword = "NewPassword456!";
            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
            };

            // Act
            LogInfo($"Changing password for user {userId}.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}/password", changePasswordRequest, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Password change was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task ChangePassword_ReturnsBadRequest_WhenPasswordIsIncorrect()
        {
            // Arrange
            // Can use seeded user here since we expect failure and won't change the password
            var userId = _aliceId;
            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword!",
                NewPassword = "NewPassword456!",
            };

            // Act
            LogInfo($"Attempting to change password for user {userId} with incorrect current password.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}/password", changePasswordRequest, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UploadAvatar_ReturnsSuccess_WhenFileIsValid()
        {
            // Arrange
            var userId = _aliceId;
            var fileContent = Encoding.UTF8.GetBytes("This is a dummy file.");
            var fileName = "test.txt";
            var contentType = "text/plain";
            var formName = "profilePicture";

            // Act
            LogInfo($"Uploading avatar for user {userId}.");
            var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-avatar/{userId}", fileContent, fileName, contentType, formName, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<AvatarUploadResponseData>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Avatar upload was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data?.ProfilePictureUrl, "Profile picture URL is returned");

            // Cleanup: Delete the uploaded avatar to keep the test environment clean.
            LogInfo($"Deleting avatar for user {userId} for cleanup.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/userprofile/delete-avatar/{userId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Cleanup status code is 200 OK");
            var apiDeleteResponse = await _api.LogDeserializeJson<object>(deleteResponse);
            await AssertHelper.AssertTrue(apiDeleteResponse.Success, "Avatar deletion for cleanup was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UploadAvatar_ReturnsBadRequest_WhenFileIsMissing()
        {
            // Arrange
            var userId = _aliceId;

            // Act
            LogInfo($"Attempting to upload avatar for user {userId} without a file.");
            var response = await _api.PostAsync<object>($"/api/v1/userprofile/upload-avatar/{userId}", null, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task DeleteAvatar_ReturnsSuccess_WhenAvatarExists()
        {
            // Arrange
            var userId = _aliceId;
            var fileContent = Encoding.UTF8.GetBytes("Dummy avatar content.");
            await _api.PostMultipartAsync($"/api/v1/userprofile/upload-avatar/{userId}", fileContent, "avatar.txt", "text/plain", "profilePicture");

            // Act
            LogInfo($"Deleting avatar for user {userId}.");
            var response = await _api.DeleteAsync($"/api/v1/userprofile/delete-avatar/{userId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Avatar deletion was successful");
        }
        
        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UpdateProfile_ReturnsSuccess_WhenFullNameIsLong()
        {
            // Arrange
            var userId = _aliceId;
            var longName = "Test User " + new string('A', 100); // Boundary check
            var updateRequest = new UpdateProfileRequest { FullName = longName };

            // Act
            LogInfo($"Updating profile with long name for user {userId}.");
            var response = await _api.PutAsync($"/api/v1/userprofile/{userId}", updateRequest, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<UserDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Update was successful");
            await AssertHelper.AssertEqual(longName, apiResponse.Data!.FullName, "Full name was updated correctly");
        }

        // [Fact]
        // [Trait("Category", "API")]
        // [Trait("Category", "UserProfile")]
        // public async Task UploadCV_ReturnsSuccess_WhenFileIsValid()
        // {
        //     // Arrange
        //     var (userId, _, _) = await CreateTestUserAsync();
        //     var fileContent = Encoding.UTF8.GetBytes("This is a dummy CV file for testing.");
        //     var fileName = "cv.pdf";
        //     var contentType = "application/pdf";
        //     var formName = "file"; // This must match the IFormFile parameter name in the controller action
        //
        //     // Act
        //     LogInfo($"Uploading CV for user {userId}.");
        //     var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-cv/{userId}", fileContent, fileName, contentType, formName, logBody: true);
        //
        //     // Assert
        //     await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        //     var apiResponse = await _api.LogDeserializeJson<string>(response); // The 'data' field is the URL string
        //     await AssertHelper.AssertTrue(apiResponse.Success, "CV upload was successful");
        //     await AssertHelper.AssertNotNull(apiResponse.Data, "CV URL is returned");
        //     await AssertHelper.AssertContains(userId.ToString(), apiResponse.Data!, "CV URL contains the user ID");
        // }

        private class AvatarUploadResponseData
        { 
            public string ProfilePictureUrl { get; set; }
        }
    }
}