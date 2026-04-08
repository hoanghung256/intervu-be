using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CoachProfileController
{
    public class ViewOwnCoachProfileTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly string _bobEmail = "bob@example.com";

        public ViewOwnCoachProfileTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task GetOwnInterviewerProfile_ReturnsSuccess_WhenCoachIsAuthenticated()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_bobEmail);

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{userId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CoachProfileDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertEqual(userId, apiResponse.Data!.Id, "Returned ID matches requested ID");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetOwnInterviewerProfile_NonExistentId_ReturnsOkWithMessage()
        {
            // Arrange - Controller catches exceptions and returns OK with message
            var (token, _) = await LoginSeededUserAsync(_bobEmail);
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{nonExistentId}", jwtToken: token, logBody: true);

            // Assert - Controller returns OK even on error (catches exception, returns message)
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Non-existent ID returns 200 OK with message");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetOwnInterviewerProfile_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var (_, userId) = await LoginSeededUserAsync(_bobEmail);

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{userId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetOwnInterviewerProfile_ReturnsForbidden_WhenCandidateAccesses()
        {
            // Arrange - Login as candidate (alice)
            var (candidateToken, _) = await LoginSeededUserAsync("alice@example.com");
            var (_, coachId) = await LoginSeededUserAsync(_bobEmail);

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{coachId}", jwtToken: candidateToken, logBody: true);

            // Assert - Endpoint requires Interviewer policy
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Candidate accessing coach profile returns 403 Forbidden");
        }
    }
}
