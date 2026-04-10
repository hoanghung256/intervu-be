using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CandidateProfileController
{
    public class DeleteCVTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public DeleteCVTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginCandidateAsync()
        {
            var email = $"candidate_{Guid.NewGuid()}@example.com";

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD,
                FullName = "Test Candidate",
                SlugProfileUrl = $"slug-{Guid.NewGuid()}"
            });

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<string> LoginAdminAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        // ===== [N] Normal / Happy Path Tests =====
        // Tests admin-only delete profile endpoint (which effectively removes candidate data)

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task DeleteCandidateProfile_ReturnsSuccess_WhenAdminDeletes()
        {
            // Arrange
            var adminToken = await LoginAdminAsync();
            var (_, candidateId) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.DeleteAsync($"/api/v1/candidate-profile/{candidateId}", jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Admin delete returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Delete was successful");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task DeleteCandidateProfile_NonExistentId_ReturnsOk_CurrentBehavior()
        {
            // Arrange - Controller catches exception and returns OK
            var adminToken = await LoginAdminAsync();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _api.DeleteAsync($"/api/v1/candidate-profile/{nonExistentId}", jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Non-existent delete returns 200 OK with message");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task DeleteCandidateProfile_ReturnsForbidden_WhenCandidateTriesToDelete()
        {
            // Arrange
            var (candidateToken, candidateId) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.DeleteAsync($"/api/v1/candidate-profile/{candidateId}", jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Candidate delete returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task DeleteCandidateProfile_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var (_, candidateId) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.DeleteAsync($"/api/v1/candidate-profile/{candidateId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token delete returns 401 Unauthorized");
        }
    }
}
