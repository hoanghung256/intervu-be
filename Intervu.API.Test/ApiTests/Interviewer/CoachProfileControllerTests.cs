using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Intervu.Application.DTOs.Coach;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Interviewer
{
    public class CoachProfileControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CoachProfileControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        // Seeded Data
        private readonly Guid _bobId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
        private readonly string _bobEmail = "bob@example.com";
        private readonly string _bobSlug = "bob-Coach_1719000000002";

        private readonly Guid _johnId = Guid.Parse("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"); // Another coach for Admin tests
        
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
        [Trait("Category", "CoachProfile")]
        public async Task GetOwnInterviewerProfile_ReturnsSuccess_WhenCoachIsAuthenticated()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_bobEmail);

            // Act
            LogInfo($"Getting own profile for coach {userId}.");
            var response = await _api.GetAsync($"/api/v1/coach-profile/{userId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CoachProfileDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertEqual(userId, apiResponse.Data!.Id, "Returned ID matches requested ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetProfileByCandidate_ReturnsSuccess_WhenSlugExists()
        {
            // Arrange
            var slug = _bobSlug;

            // Act
            LogInfo($"Getting public profile for slug '{slug}'.");
            var response = await _api.GetAsync($"/api/v1/coach-profile/{slug}/profile", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CoachProfileDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Profile data returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachProfile_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_bobEmail);
            
            var updateDto = new CoachUpdateDto
            {
                // Id = userId,
                FullName = "Updated Bob", 
                Email = "updated@example.com",
                Bio = "Updated Bio for testing purposes.",
                ExperienceYears = 9,
                PortfolioUrl = "https://updated-portfolio.example.com",
                CurrentAmount = 100
            };

            // Act
            LogInfo($"Updating profile for coach {userId}.");
            var response = await _api.PutAsync($"/api/v1/coach-profile/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Update was successful");
            await AssertHelper.AssertEqual("Profile updated successfully", apiResponse.Message, "Message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachStatus_ReturnsSuccess_WhenAdminIsAuthenticated()
        {
            // Arrange
            var (adminToken, _) = await LoginSeededUserAsync(_adminEmail);
            var targetCoachId = _johnId; // Use John so we don't mess up Bob's active status for other tests if possible
            var newStatus = 1; // Enable (assuming enum value 1 based on context)

            // Act
            LogInfo($"Admin updating status for coach {targetCoachId} to {newStatus}.");
            // Note: The controller expects [FromBody] CoachProfileStatus. 
            // We send the integer value directly as the body.
            var response = await _api.PutAsync($"/api/v1/coach-profile/{targetCoachId}/status", newStatus, jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Status update was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachStatus_ReturnsForbidden_WhenUserIsNotAdmin()
        {
            // Arrange
            var (coachToken, coachId) = await LoginSeededUserAsync(_bobEmail);
            var newStatus = 1;

            // Act
            LogInfo("Coach attempting to update their own status (should fail).");
            var response = await _api.PutAsync($"/api/v1/coach-profile/{coachId}/status", newStatus, jwtToken: coachToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden");
        }
    }
}