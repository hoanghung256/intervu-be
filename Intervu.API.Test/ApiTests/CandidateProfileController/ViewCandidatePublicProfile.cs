using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CandidateProfileController
{
    public class ViewCandidatePublicProfileTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewCandidatePublicProfileTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> RegisterAndGetSlugAsync()
        {
            var email = $"candidate_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD,
                FullName = "Test Candidate",
                SlugProfileUrl = $"slug-{Guid.NewGuid()}"
            }, logBody: true);

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);
            return loginData.Data!.User.SlugProfileUrl;
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetProfileBySlug_ReturnsSuccess_WhenSlugExists()
        {
            // Arrange
            var slug = await RegisterAndGetSlugAsync();

            // Act
            var response = await _api.GetAsync($"/api/v1/candidate-profile/{slug}/profile", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CandidateProfileDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Profile data returned");
            await AssertHelper.AssertEqual(slug, apiResponse.Data!.User.SlugProfileUrl, "Slug matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetProfileBySlug_ReturnsProfileFields_WhenSlugExists()
        {
            // Arrange
            var slug = await RegisterAndGetSlugAsync();

            // Act
            var response = await _api.GetAsync($"/api/v1/candidate-profile/{slug}/profile", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CandidateProfileDto>(response);
            await AssertHelper.AssertNotNull(apiResponse.Data, "Profile data returned");
            await AssertHelper.AssertNotNull(apiResponse.Data!.User, "User data is present in profile");
            await AssertHelper.AssertNotNull(apiResponse.Data.User.FullName, "FullName is present");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetProfileBySlug_NonExistentSlug_ReturnsOkWithMessage()
        {
            // Arrange - Controller catches exceptions and returns OK with error message
            var nonExistentSlug = $"non-existent-slug-{Guid.NewGuid()}";

            // Act
            var response = await _api.GetAsync($"/api/v1/candidate-profile/{nonExistentSlug}/profile", logBody: true);

            // Assert - Controller always returns 200 OK (catches exception, returns message)
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Non-existent slug returns 200 OK");
        }

        // ===== [A] Abnormal / Error Path Tests =====
        // Note: This endpoint is [AllowAnonymous], so no auth tests needed

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetProfileBySlug_NoAuthRequired_PublicEndpoint()
        {
            // Arrange
            var slug = await RegisterAndGetSlugAsync();

            // Act - No auth token
            var response = await _api.GetAsync($"/api/v1/candidate-profile/{slug}/profile", logBody: true);

            // Assert - Should work without auth (AllowAnonymous)
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Public endpoint returns 200 OK without auth");
        }
    }
}
