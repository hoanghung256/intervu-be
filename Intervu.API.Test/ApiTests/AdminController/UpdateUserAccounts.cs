using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class UpdateUserAccountsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public UpdateUserAccountsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var data = await _api.LogDeserializeJson<LoginResponse>(response);
            return data.Data!.Token;
        }

        private async Task<(Guid Id, string Email)> CreateTestUserAsync(string token, UserRole role = UserRole.Candidate)
        {
            var email = $"update_test_{Guid.NewGuid()}@example.com";
            var createDto = new AdminCreateUserDto
            {
                FullName = "Update Test User",
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
        public async Task Handle_ValidRequest_UpdatesUserAccount()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"upd_{Guid.NewGuid()}@example.com";
            var create = await _api.PostAsync("/api/v1/admin/users", new AdminCreateUserDto { FullName = "Temp", Email = email, Password = CANDIDATE_PASSWORD, Role = UserRole.Candidate, Status = UserStatus.Active }, jwtToken: token);
            var user = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(create);

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{user.Data!.Id}", new AdminCreateUserDto { FullName = "Temp Updated", Email = email, Role = UserRole.Candidate, Status = UserStatus.Active }, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_ValidData_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, email) = await CreateTestUserAsync(token);

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Managed User Updated",
                Email = email,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update returns 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(updateResponse);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_ValidData_ReturnsUpdatedFullName()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, email) = await CreateTestUserAsync(token);
            var newFullName = "FullName Updated Verified";

            var updateDto = new AdminCreateUserDto
            {
                FullName = newFullName,
                Email = email,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(updateResponse);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertEqual(newFullName, result.Data!.FullName, "FullName matches updated value");
            await AssertHelper.AssertEqual(email, result.Data.Email, "Email remains unchanged");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_ChangeEmail_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, _) = await CreateTestUserAsync(token);
            var newEmail = $"changed_{Guid.NewGuid()}@example.com";

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Email Changed User",
                Email = newEmail,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update with new email returns 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(updateResponse);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertEqual(newEmail, result.Data!.Email, "Email matches new value");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_ChangePassword_CanLoginWithNewPassword()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, email) = await CreateTestUserAsync(token);
            var newPassword = "newPass@99999";

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Password Changed User",
                Email = email,
                Password = newPassword,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update with new password returns 200 OK");

            // Assert - verify login with new password works
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = newPassword });
            await AssertHelper.AssertEqual(HttpStatusCode.OK, loginResponse.StatusCode, "Login with new password returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_ChangeRoleCandidateToCoach_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, email) = await CreateTestUserAsync(token, UserRole.Candidate);

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Role Changed To Coach",
                Email = email,
                Role = UserRole.Coach,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Role change Candidate->Coach returns 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(updateResponse);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertEqual(UserRole.Coach, result.Data!.Role, "Role is updated to Coach");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_ChangeRoleCandidateToAdmin_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, email) = await CreateTestUserAsync(token, UserRole.Candidate);

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Role Changed To Admin",
                Email = email,
                Role = UserRole.Admin,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Role change Candidate->Admin returns 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(updateResponse);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertEqual(UserRole.Admin, result.Data!.Role, "Role is updated to Admin");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_ChangeStatusToSuspended_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, email) = await CreateTestUserAsync(token);

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Suspended User",
                Email = email,
                Role = UserRole.Candidate,
                Status = UserStatus.Suspended
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Status change to Suspended returns 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(updateResponse);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertEqual(UserStatus.Suspended, result.Data!.Status, "Status is updated to Suspended");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_NonExistentUserId_ReturnsBadRequest()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var nonExistentId = Guid.NewGuid();

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Ghost User",
                Email = "ghost@example.com",
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{nonExistentId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, updateResponse.StatusCode, "Non-existent user returns 400 BadRequest");
            var result = await _api.LogDeserializeJson<object>(updateResponse);
            await AssertHelper.AssertFalse(result.Success, "Response success is false");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_EmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var token = await LoginAdminAsync();

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Empty Guid User",
                Email = "emptyguid@example.com",
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{Guid.Empty}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, updateResponse.StatusCode, "Empty GUID returns 400 BadRequest");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId1, _) = await CreateTestUserAsync(token);
            var (_, existingEmail) = await CreateTestUserAsync(token);

            var updateDto = new AdminCreateUserDto
            {
                FullName = "Duplicate Email User",
                Email = existingEmail,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId1}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, updateResponse.StatusCode, "Duplicate email returns 400 BadRequest");
            var result = await _api.LogDeserializeJson<object>(updateResponse);
            await AssertHelper.AssertFalse(result.Success, "Response success is false");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var (userId, email) = await CreateTestUserAsync(token);

            var updateDto = new AdminCreateUserDto
            {
                FullName = "No Auth User",
                Email = email,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, updateResponse.StatusCode, "Missing auth token returns 401 Unauthorized");
        }
    }
}
