using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CoachProfileController
{
    public class UpdateCoachProfileTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly string _bobEmail = "bob@example.com";

        public UpdateCoachProfileTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task UpdateCoachProfile_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_bobEmail);

            var updateDto = new CoachUpdateDto
            {
                FullName = "Updated Bob",
                Email = "updated@example.com",
                Bio = "Updated Bio for testing purposes.",
                ExperienceYears = 9,
                PortfolioUrl = "https://updated-portfolio.example.com",
                CurrentAmount = 100
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-profile/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Update was successful");
            await AssertHelper.AssertEqual("Profile updated successfully!", apiResponse.Message, "Message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachProfile_PartialUpdate_ReturnsSuccess()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_bobEmail);

            var updateDto = new CoachUpdateDto
            {
                FullName = "Bob Partial Update",
                Email = "bob@example.com",
                Bio = "Partially updated bio only"
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-profile/{userId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Partial update returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Partial update was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task ManageCoachWorkExperiences_FullCRUDCycle_ReturnsSuccess()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_bobEmail);

            // Create
            var createDto = new CoachWorkExperienceDto
            {
                CompanyName = "Test Corp",
                PositionTitle = "Senior Engineer",
                StartDate = DateTime.UtcNow.AddYears(-2),
                IsCurrentWorking = true
            };

            var createResponse = await _api.PostAsync($"/api/v1/coach-profile/{userId}/work-experiences", createDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create work experience returns 200 OK");
            var createResult = await _api.LogDeserializeJson<CoachWorkExperienceDto>(createResponse);
            var workId = createResult.Data!.Id;

            // Update
            var updateDto = new CoachWorkExperienceDto
            {
                CompanyName = "Updated Corp",
                PositionTitle = "Lead Engineer",
                StartDate = DateTime.UtcNow.AddYears(-2),
                IsCurrentWorking = true
            };

            var updateResponse = await _api.PutAsync($"/api/v1/coach-profile/{userId}/work-experiences/{workId}", updateDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update work experience returns 200 OK");

            // Delete
            var deleteResponse = await _api.DeleteAsync($"/api/v1/coach-profile/{userId}/work-experiences/{workId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete work experience returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task ManageCoachCertificates_FullCRUDCycle_ReturnsSuccess()
        {
            // Arrange
            var (token, userId) = await LoginSeededUserAsync(_bobEmail);

            // Create
            var addResponse = await _api.PostAsync($"/api/v1/coach-profile/{userId}/certificates", new CoachCertificateDto
            {
                Name = "AWS Solutions Architect",
                Issuer = "Amazon Web Services",
                IssuedAt = DateTime.UtcNow.AddMonths(-6)
            }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, addResponse.StatusCode, "Add certificate returns 200 OK");
            var addResult = await _api.LogDeserializeJson<CoachCertificateDto>(addResponse);
            var certId = addResult.Data!.Id;

            // Update
            var updateResponse = await _api.PutAsync($"/api/v1/coach-profile/{userId}/certificates/{certId}", new CoachCertificateDto
            {
                Name = "AWS Solutions Architect Professional",
                Issuer = "Amazon Web Services",
                IssuedAt = DateTime.UtcNow.AddMonths(-6)
            }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update certificate returns 200 OK");

            // Delete
            var deleteResponse = await _api.DeleteAsync($"/api/v1/coach-profile/{userId}/certificates/{certId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete certificate returns 200 OK");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachProfile_NonExistentCoachId_ReturnsBadRequest()
        {
            // Arrange
            var (token, _) = await LoginSeededUserAsync(_bobEmail);
            var nonExistentId = Guid.NewGuid();

            var updateDto = new CoachUpdateDto
            {
                FullName = "Ghost Coach",
                Email = "ghost@example.com",
                Bio = "This coach doesn't exist"
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-profile/{nonExistentId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Non-existent coach returns 400 BadRequest");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachProfile_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var (_, userId) = await LoginSeededUserAsync(_bobEmail);
            var updateDto = new CoachUpdateDto
            {
                FullName = "Unauthorized Update",
                Email = "unauthorized@example.com",
                Bio = "Unauthorized update payload",
                ExperienceYears = 3,
                PortfolioUrl = "https://unauthorized.example.com",
                CurrentAmount = 1
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-profile/{userId}", updateDto, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachProfile_ReturnsForbidden_WhenCandidateAccesses()
        {
            // Arrange
            var (candidateToken, _) = await LoginSeededUserAsync("alice@example.com");
            var (_, coachId) = await LoginSeededUserAsync(_bobEmail);

            var updateDto = new CoachUpdateDto
            {
                FullName = "Candidate Trying Update",
                Email = "candidate@example.com",
                Bio = "Should not work"
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-profile/{coachId}", updateDto, jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Candidate updating coach returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task CreateWorkExperience_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var (_, userId) = await LoginSeededUserAsync(_bobEmail);

            // Act
            var response = await _api.PostAsync($"/api/v1/coach-profile/{userId}/work-experiences", new CoachWorkExperienceDto
            {
                CompanyName = "No Auth Corp",
                PositionTitle = "Dev",
                StartDate = DateTime.UtcNow.AddYears(-1),
                IsCurrentWorking = true
            }, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Create work experience without token returns 401");
        }
    }
}
