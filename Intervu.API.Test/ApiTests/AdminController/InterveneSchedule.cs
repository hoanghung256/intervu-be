using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class InterveneScheduleTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public InterveneScheduleTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        /// <summary>
        /// Admin can suspend a user and then activate them — full lifecycle test for schedule intervention.
        /// </summary>
        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ActivateUser_SuspendedUser_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"intervene_{Guid.NewGuid()}@example.com";
            var createDto = new AdminCreateUserDto
            {
                FullName = "Intervene User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };
            var createResponse = await _api.PostAsync("/api/v1/admin/users", createDto, jwtToken: token);
            var createdUser = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(createResponse);
            var userId = createdUser.Data!.Id;

            // Suspend the user first
            await _api.DeleteAsync($"/api/v1/admin/users/{userId}", jwtToken: token);

            // Act
            var activateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}/activate", new { }, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, activateResponse.StatusCode, "Activate suspended user returns 200 OK");
            var result = await _api.LogDeserializeJson<object>(activateResponse);
            await AssertHelper.AssertTrue(result.Success, "Activate response success is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ActivateUser_AlreadyActiveUser_ReturnsNotFound()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"intervene_active_{Guid.NewGuid()}@example.com";
            var createDto = new AdminCreateUserDto
            {
                FullName = "Already Active User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };
            var createResponse = await _api.PostAsync("/api/v1/admin/users", createDto, jwtToken: token);
            var createdUser = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(createResponse);
            var userId = createdUser.Data!.Id;

            // Act - Try to activate an already active user
            var activateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}/activate", new { }, jwtToken: token, logBody: true);

            // Assert - ActivateUserForAdmin returns false → controller returns NotFound
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, activateResponse.StatusCode, "Already active user returns 404 NotFound");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ActivateUser_NonExistentUserId_ReturnsNotFound()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _api.PutAsync($"/api/v1/admin/users/{nonExistentId}/activate", new { }, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent user activate returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ActivateUser_EmptyGuid_ReturnsNotFound()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.PutAsync($"/api/v1/admin/users/{Guid.Empty}/activate", new { }, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Empty GUID activate returns 404 NotFound");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ActivateUser_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.PutAsync($"/api/v1/admin/users/{Guid.NewGuid()}/activate", new { }, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No auth token returns 401 Unauthorized");
        }
    }
}
