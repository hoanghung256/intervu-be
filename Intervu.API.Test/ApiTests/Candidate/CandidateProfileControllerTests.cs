using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Intervu.Application.DTOs.Candidate;
using Intervu.Domain.Entities.Constants;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Candidate
{
    public class CandidateProfileControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CandidateProfileControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        // Seeded Data
        private readonly Guid _aliceId = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
        private readonly string _aliceEmail = "alice@example.com";
        private readonly string _aliceSlug = "alice-candidate_1719000000001";

        private readonly string _adminEmail = "admin@example.com";

        private async Task<(string token, Guid userId)> LoginSeededUserAsync(string email)
        {
            var password = ACCOUNT_PASSWORD;

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            if (!loginData.Success)
            {
                throw new Exception($"Failed to login seeded user {email}.");
            }

            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetOwnCandidateProfile_ReturnsSuccess_WhenCandidateIsAuthenticated()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_aliceEmail);

            // Act
            LogInfo($"Getting own profile for candidate {userId}.");
            var response = await _api.GetAsync($"/api/v1/candidate-profile/{userId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CandidateProfileDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertEqual(userId, apiResponse.Data!.Id, "Returned ID matches requested ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetProfileBySlug_ReturnsSuccess_WhenSlugExists()
        {
            // Arrange
            var slug = _aliceSlug;

            // Act
            LogInfo($"Getting public profile for slug '{slug}'.");
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
        public async Task UpdateCandidateProfile_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_aliceEmail);

            var updateDto = new CandidateUpdateDto
            {
                // Id = userId,
                FullName = "Alice Updated",
                Email = "updated@example.com",
                Bio = "Updated Bio for testing purposes.",
                PortfolioUrl = "https://updated-portfolio.example.com/alice",
                CVUrl = "https://updated-cv.example.com/alice.pdf"
            };

            // Act
            LogInfo($"Updating profile for candidate {userId}.");
            var response = await _api.PutAsync($"/api/v1/candidate-profile/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Update was successful");
            await AssertHelper.AssertEqual("Profile updated successfully", apiResponse.Message, "Message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task UpdateCandidateStatus_ReturnsSuccess_WhenAdminIsAuthenticated()
        {
            // Arrange
            var (adminToken, _) = await LoginSeededUserAsync(_adminEmail);
            var targetCandidateId = _aliceId;
            var newStatus = 0; // Active

            // Act
            LogInfo($"Admin updating status for candidate {targetCandidateId} to {newStatus}.");
            var response = await _api.PutAsync($"/api/v1/candidate-profile/{targetCandidateId}/status", newStatus, jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Status update was successful");
            await AssertHelper.AssertEqual("Status updated successfully", apiResponse.Message, "Message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task UpdateCandidateStatus_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var (candidateToken, candidateId) = await LoginSeededUserAsync(_aliceEmail);
            var newStatus = 0; // Inactive

            // Act
            LogInfo("Candidate attempting to update their own status (should fail).");
            var response = await _api.PutAsync($"/api/v1/candidate-profile/{candidateId}/status", newStatus, jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden");
        }
    }
}