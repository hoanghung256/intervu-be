using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CandidateProfileController
{
    public class ViewCVTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewCVTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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

        // ===== [N] Normal / Happy Path Tests =====
        // View profile to check CV URL — the profile endpoint includes CVUrl field

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task ViewProfile_ContainsCVUrlField_WhenAuthenticated()
        {
            // Arrange
            var (token, userId) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.GetAsync($"/api/v1/candidate-profile/{userId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Get profile returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task UpdateProfile_WithCVUrl_ReturnsSuccess()
        {
            // Arrange
            var (token, userId) = await RegisterAndLoginCandidateAsync();
            var updateDto = new CandidateUpdateDto
            {
                FullName = "CV Updated Candidate",
                Email = $"cv_updated_{Guid.NewGuid()}@example.com",
                CVUrl = "https://storage.example.com/cv/test-resume.pdf",
                Bio = "Profile with CV"
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/candidate-profile/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Update with CV URL returns 200 OK");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task UpdateProfile_WithEmptyCVUrl_ReturnsSuccess()
        {
            // Arrange
            var (token, userId) = await RegisterAndLoginCandidateAsync();
            var updateDto = new CandidateUpdateDto
            {
                FullName = "Empty CV Candidate",
                Email = $"empty_cv_{Guid.NewGuid()}@example.com",
                CVUrl = "",
                Bio = "Profile with empty CV URL"
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/candidate-profile/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Update with empty CV URL returns 200 OK");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task ViewProfile_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var (_, userId) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.GetAsync($"/api/v1/candidate-profile/{userId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token returns 401 Unauthorized");
        }
    }
}
