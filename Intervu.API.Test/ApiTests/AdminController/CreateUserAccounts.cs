using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    // IC-9
    public class CreateUserAccountsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public CreateUserAccountsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var data = await _api.LogDeserializeJson<LoginResponse>(response);
            return data.Data!.Token;
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task CreateUser_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_created_{Guid.NewGuid()}@example.com";
            var createUserDto = new AdminCreateUserDto
            {
                FullName = "Managed User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            LogInfo($"Creating user {email}.");
            var createResponse = await _api.PostAsync("/api/v1/admin/users", createUserDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");
            var createResult = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(createResponse);
            await AssertHelper.AssertNotNull(createResult.Data, "Created user data is not null");
            await AssertHelper.AssertEqual(email, createResult.Data!.Email, "Created user email matches request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task Handle_ValidRequest_CreatesUserAccount()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_created_{Guid.NewGuid()}@example.com";

            // Act
            var response = await _api.PostAsync("/api/v1/admin/users", new AdminCreateUserDto { FullName = "Managed User", Email = email, Password = CANDIDATE_PASSWORD, Role = UserRole.Candidate, Status = UserStatus.Active }, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Create status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task CreateUser_CoachRole_ReturnsCoachUser()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_created_coach_{Guid.NewGuid()}@example.com";
            var request = new AdminCreateUserDto
            {
                FullName = "Managed Coach",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Coach,
                Status = UserStatus.Active
            };

            // Act
            var response = await _api.PostAsync("/api/v1/admin/users", request, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Create coach status code is 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(response);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertEqual(UserRole.Coach, result.Data!.Role, "Created role is Coach");
            await AssertHelper.AssertEqual(email, result.Data.Email, "Created coach email matches request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task CreateUser_AdminRole_ReturnsAdminUser()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_created_admin_{Guid.NewGuid()}@example.com";
            var request = new AdminCreateUserDto
            {
                FullName = "Managed Admin",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Admin,
                Status = UserStatus.Active
            };

            // Act
            var response = await _api.PostAsync("/api/v1/admin/users", request, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Create admin status code is 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(response);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertEqual(UserRole.Admin, result.Data!.Role, "Created role is Admin");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task CreateUser_StatusOmitted_DefaultsToActive()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_created_default_status_{Guid.NewGuid()}@example.com";
            var request = new AdminCreateUserDto
            {
                FullName = "Default Status User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate
            };

            // Act
            var response = await _api.PostAsync("/api/v1/admin/users", request, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Create with omitted status returns 200 OK");
            var result = await _api.LogDeserializeJson<AdminUserResponseDto>(response);
            await AssertHelper.AssertTrue(result.Success, "Response success is true");
            await AssertHelper.AssertEqual(UserStatus.Active, result.Data!.Status, "Default status is Active");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task CreateUser_VerifyCreatedUserCanLogin()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_created_login_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            var request = new AdminCreateUserDto
            {
                FullName = "Login Verify User",
                Email = email,
                Password = password,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var createResponse = await _api.PostAsync("/api/v1/admin/users", request, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create returns 200 OK");

            // Assert - verify the created user can login
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            await AssertHelper.AssertEqual(HttpStatusCode.OK, loginResponse.StatusCode, "Created user can login successfully");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task CreateUser_NullPassword_ReturnsInternalServerError()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_created_null_password_{Guid.NewGuid()}@example.com";
            var request = new AdminCreateUserDto
            {
                FullName = "Null Password User",
                Email = email,
                Password = null,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var response = await _api.PostAsync("/api/v1/admin/users", request, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Null password returns 500 InternalServerError");
            var result = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(result.Success, "Response success is false");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task CreateUser_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_created_duplicate_{Guid.NewGuid()}@example.com";
            var firstCreateRequest = new AdminCreateUserDto
            {
                FullName = "First User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            var duplicateCreateRequest = new AdminCreateUserDto
            {
                FullName = "Second User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            await _api.PostAsync("/api/v1/admin/users", firstCreateRequest, jwtToken: token, logBody: true);

            // Act
            var duplicateResponse = await _api.PostAsync("/api/v1/admin/users", duplicateCreateRequest, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, duplicateResponse.StatusCode, "Duplicate email returns 400 BadRequest");
            var result = await _api.LogDeserializeJson<object>(duplicateResponse);
            await AssertHelper.AssertFalse(result.Success, "Response success is false");
            await AssertHelper.AssertContains("registered", result.Message ?? string.Empty, "Error message indicates duplicate registration");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task CreateUser_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Arrange
            var email = $"admin_created_noauth_{Guid.NewGuid()}@example.com";
            var request = new AdminCreateUserDto
            {
                FullName = "No Auth User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // Act
            var response = await _api.PostAsync("/api/v1/admin/users", request, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Missing auth token returns 401 Unauthorized");
        }
    }
}
