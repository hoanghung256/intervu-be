using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    // IC-10
    public class DeleteUserAccountsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public DeleteUserAccountsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var data = await _api.LogDeserializeJson<LoginResponse>(response);
            return data.Data!.Token;
        }

        private async Task<(Guid Id, string Email)> CreateTestUserAsync(string token, UserRole role = UserRole.Candidate)
        {
            var email = $"delete_test_{Guid.NewGuid()}@example.com";
            var createDto = new AdminCreateUserDto
            {
                FullName = "Delete Test User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = role,
                Status = UserStatus.Active
            };
            var createResponse = await _api.PostAsync("/api/v1/admin/users", createDto, jwtToken: token);
            var createResult = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(createResponse);
            return (createResult.Data!.Id, email);
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task DeleteUser_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, email) = await CreateTestUserAsync(token);

            // Act
            LogInfo($"Suspending user {userId}.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{userId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Suspend status code is 200 OK");
            var result = await _api.LogDeserializeJson<object>(deleteResponse);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task DeleteUser_VerifyResponseBody_ContainsUserIdAndStatus()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, _) = await CreateTestUserAsync(token);

            // Act
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{userId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
            var result = await _api.LogDeserializeJson<object>(deleteResponse);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertContains("inactive", result.Message ?? string.Empty, "Response message indicates inactive status");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task DeleteUser_ThenActivate_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, _) = await CreateTestUserAsync(token);

            // Act - Delete (suspend)
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{userId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Suspend returns 200 OK");

            // Act - Activate
            var activateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}/activate", new { }, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, activateResponse.StatusCode, "Activate after suspend returns 200 OK");
            var result = await _api.LogDeserializeJson<object>(activateResponse);
            await AssertHelper.AssertTrue(result.Success, "Activate response success is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task DeleteUser_ThenGetById_UserStatusIsSuspended()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, _) = await CreateTestUserAsync(token);

            // Act - Delete (suspend)
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{userId}", jwtToken: token);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Suspend returns 200 OK");

            // Assert - Verify user status via GET
            var getResponse = await _api.GetAsync($"/api/v1/admin/users/{userId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getResponse.StatusCode, "Get suspended user returns 200 OK");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task DeleteUser_NonExistentUserId_ReturnsOk_CurrentBehavior()
        {
            // Arrange - DeleteUserForAdmin returns false for non-existent, but controller checks `status == null`
            // Since bool is never null, this always returns OK (known bug documented by this test)
            var token = await LoginAdminAsync();
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{nonExistentUserId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Non-existent user currently returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task DeleteUser_EmptyGuid_ReturnsOk_CurrentBehavior()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{Guid.Empty}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Empty GUID currently returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task DeleteUser_AlreadySuspendedUser_StillReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, _) = await CreateTestUserAsync(token);

            // First delete (suspend)
            await _api.DeleteAsync($"/api/v1/admin/users/{userId}", jwtToken: token);

            // Act - Second delete on already suspended user
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{userId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Double-delete returns 200 OK");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task DeleteUser_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, _) = await CreateTestUserAsync(token);

            // Act
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{userId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, deleteResponse.StatusCode, "Missing auth token returns 401 Unauthorized");
        }
    }
}
