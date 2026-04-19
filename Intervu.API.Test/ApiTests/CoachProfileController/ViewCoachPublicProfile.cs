using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Coach;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CoachProfileController
{
    public class ViewCoachPublicProfileTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly string _bobSlug = "bob-Coach_1719000000002";

        public ViewCoachPublicProfileTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetProfileByCandidate_ReturnsSuccess_WhenSlugExists()
        {
            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{_bobSlug}/profile", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CoachProfileDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Profile data returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetProfileByCandidate_ReturnsProfileData_WithCorrectFields()
        {
            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{_bobSlug}/profile", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CoachProfileDto>(response);
            await AssertHelper.AssertNotNull(apiResponse.Data, "Profile data returned");
            await AssertHelper.AssertNotNull(apiResponse.Data!.Id, "Profile ID is present");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetCoachRating_ReturnsSuccess_PublicEndpoint()
        {
            // Arrange - Rating endpoint is public (no auth needed based on controller)
            // Use a known coach ID — get it from the profile
            var profileResponse = await _api.GetAsync($"/api/v1/coach-profile/{_bobSlug}/profile");
            var profile = await _api.LogDeserializeJson<CoachProfileDto>(profileResponse);
            var coachId = profile.Data!.Id;

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{coachId}/rating", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Coach rating returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Rating request was successful");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetProfileByCandidate_NonExistentSlug_ReturnsOkWithMessage()
        {
            // Arrange - Controller catches exceptions and returns OK with error message
            var nonExistentSlug = $"non-existent-slug-{Guid.NewGuid()}";

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{nonExistentSlug}/profile", logBody: true);

            // Assert - Controller always returns 200 OK even on error (returns message only)
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Non-existent slug returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetCoachRating_NonExistentCoachId_ReturnsSuccessWithNoRating()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-profile/{nonExistentId}/rating", logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Non-existent coach rating returns 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Response success flag is true");
            await AssertHelper.AssertEqual(0.0, payload.Data!.GetProperty("rating").GetDouble(), "Rating should be 0.0 for non-existent coach");
            await AssertHelper.AssertEqual(0, payload.Data!.GetProperty("totalRatings").GetInt32(), "TotalRatings should be 0 for non-existent coach");
        }
    }
}
