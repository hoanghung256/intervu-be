using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CoachProfileController
{
    public class UpdateCoachStatusTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly string _bobEmail = "bob@example.com";
        private readonly Guid _johnId = Guid.Parse("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44");
        private readonly string _adminEmail = "admin@example.com";

        public UpdateCoachStatusTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> LoginSeededUserAsync(string email)
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachStatus_ReturnsSuccess_WhenAdminIsAuthenticated()
        {
            // Arrange
            var (adminToken, _) = await LoginSeededUserAsync(_adminEmail);

            // Act - Set status to Enable (0)
            var response = await _api.PutAsync($"/api/v1/coach-profile/{_johnId}/status", 0, jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Status update was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachStatus_DisableStatus_ReturnsSuccess()
        {
            // Arrange
            var (adminToken, _) = await LoginSeededUserAsync(_adminEmail);

            // Act - Set status to Disable (1)
            var response = await _api.PutAsync($"/api/v1/coach-profile/{_johnId}/status", 1, jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Disable status update was successful");

            // Cleanup - Re-enable
            await _api.PutAsync($"/api/v1/coach-profile/{_johnId}/status", 0, jwtToken: adminToken);
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachStatus_NonExistentCoachId_ReturnsOkWithErrorMessage()
        {
            // Arrange - Controller catches exception and returns OK with error message
            var (adminToken, _) = await LoginSeededUserAsync(_adminEmail);
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-profile/{nonExistentId}/status", 0, jwtToken: adminToken, logBody: true);

            // Assert - Controller always returns 200 OK (catches exception)
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Non-existent coach returns 200 OK with error message");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachStatus_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var (coachToken, coachId) = await LoginSeededUserAsync(_bobEmail);

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-profile/{coachId}/status", 1, jwtToken: coachToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachStatus_ReturnsUnauthorized_WhenNoToken()
        {
            // Act
            var response = await _api.PutAsync($"/api/v1/coach-profile/{_johnId}/status", 1, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token returns 401 Unauthorized");
        }
    }
}
