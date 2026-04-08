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

        private async Task<(string token, Guid userId, string slug)> RegisterAndLoginCandidateAsync()
        {
            var email = $"candidate_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            var fullName = "Test Candidate";
            var slug = $"slug-{Guid.NewGuid()}";

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = fullName,
                SlugProfileUrl = slug
            }, logBody: true);

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);

            slug = loginData.Data!.User.SlugProfileUrl; // Update slug with actual value from response

            return (loginData.Data!.Token, loginData.Data.User.Id, slug);
        }

        private async Task<string> LoginAdminAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetOwnCandidateProfile_ReturnsSuccess_WhenCandidateIsAuthenticated()
        {
            // Arrange
            var (token, userId, _) = await RegisterAndLoginCandidateAsync();

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
            var (_, _, slug) = await RegisterAndLoginCandidateAsync();

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
            var (token, userId, _) = await RegisterAndLoginCandidateAsync();

            var updateDto = new CandidateUpdateDto
            {
                FullName = "Candidate Updated",
                Email = "updated@example.com",
                Bio = "Updated Bio for testing purposes.",
                PortfolioUrl = "https://updated-portfolio.example.com/test",
                CVUrl = "https://updated-cv.example.com/test.pdf"
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
        public async Task GetCandidateRating_ReturnsSuccess_WhenAuthenticated()
        {
            // Arrange
            var (token, userId, _) = await RegisterAndLoginCandidateAsync();

            // Act
            LogInfo($"Getting rating for candidate {userId}.");
            var response = await _api.GetAsync($"/api/v1/candidate-profile/{userId}/rating", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task UpdateCandidateStatus_ReturnsSuccess_WhenAdminIsAuthenticated()
        {
            // Arrange
            var adminToken = await LoginAdminAsync();
            var (_, targetCandidateId, _) = await RegisterAndLoginCandidateAsync();
            var newStatus = 0; // Active

            // Act
            LogInfo($"Admin updating status for candidate {targetCandidateId} to {newStatus}.");
            var response = await _api.PutAsync($"/api/v1/candidate-profile/{targetCandidateId}/status", newStatus, jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Status update was successful");
            await AssertHelper.AssertEqual("Profile updated successfully", apiResponse.Message, "Message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task DeleteCandidateProfile_ReturnsSuccess_WhenAdminIsAuthenticated()
        {
            // Arrange
            var adminToken = await LoginAdminAsync();
            var (_, targetCandidateId, _) = await RegisterAndLoginCandidateAsync();

            // Act
            LogInfo($"Admin deleting profile for candidate {targetCandidateId}.");
            var response = await _api.DeleteAsync($"/api/v1/candidate-profile/{targetCandidateId}", jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Deletion was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task ManageWorkExperiences_ReturnsSuccess()
        {
            // Arrange
            var (token, userId, _) = await RegisterAndLoginCandidateAsync();

            // 1. Create Work Experience
            var createDto = new CandidateWorkExperienceDto
            {
                CompanyName = "Initial Corp",
                PositionTitle = "Junior dev",
                StartDate = DateTime.UtcNow.AddYears(-1),
                IsCurrentWorking = true
            };

            LogInfo("Creating work experience.");
            var createResponse = await _api.PostAsync($"/api/v1/candidate-profile/{userId}/work-experiences", createDto, jwtToken: token, logBody: true);
            var createResult = await _api.LogDeserializeJson<CandidateWorkExperienceDto>(createResponse);
            var workId = createResult.Data!.Id;
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");

            // 2. Update Work Experience
            var updateDto = new CandidateWorkExperienceDto
            {
                Id = workId,
                CompanyName = "Updated Corp",
                PositionTitle = "Senior dev",
                StartDate = DateTime.UtcNow.AddYears(-1),
                IsCurrentWorking = true
            };

            LogInfo("Updating work experience.");
            var updateResponse = await _api.PutAsync($"/api/v1/candidate-profile/{userId}/work-experiences/{workId}", updateDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");

            // 3. Batch Update (UpdateCandidateWorkExperiences)
            var batchRequest = new UpdateCandidateWorkExperiencesRequest
            {
                WorkExperiences = new List<CandidateWorkExperienceDto> { updateDto }
            };
            LogInfo("Batch updating work experiences.");
            var batchResponse = await _api.PutAsync($"/api/v1/candidate-profile/{userId}/work-experiences", batchRequest, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, batchResponse.StatusCode, "Batch update status code is 200 OK");

            // 4. Delete Work Experience
            LogInfo("Deleting work experience.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/candidate-profile/{userId}/work-experiences/{workId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task ManageCertificates_ReturnsSuccess()
        {
            // Arrange
            var (token, userId, _) = await RegisterAndLoginCandidateAsync();

            // 1. Add Certificate
            var addDto = new CandidateCertificateDto
            {
                Name = "AWS Cloud Practitioner",
                Issuer = "Amazon Web Services",
                IssuedAt = DateTime.UtcNow.AddMonths(-3)
            };

            LogInfo("Adding certificate.");
            var addResponse = await _api.PostAsync($"/api/v1/candidate-profile/{userId}/certificates", addDto, jwtToken: token, logBody: true);
            var addResult = await _api.LogDeserializeJson<CandidateCertificateDto>(addResponse);
            var certId = addResult.Data!.Id;
            await AssertHelper.AssertEqual(HttpStatusCode.OK, addResponse.StatusCode, "Add certificate status code is 200 OK");

            // 2. Update Certificate
            var updateDto = new CandidateCertificateDto
            {
                Id = certId,
                Name = "AWS Certified Developer",
                Issuer = "Amazon Web Services",
                IssuedAt = DateTime.UtcNow.AddMonths(-3)
            };

            LogInfo("Updating certificate.");
            var updateResponse = await _api.PutAsync($"/api/v1/candidate-profile/{userId}/certificates/{certId}", updateDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update certificate status code is 200 OK");

            // 3. Delete Certificate
            LogInfo("Deleting certificate.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/candidate-profile/{userId}/certificates/{certId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete certificate status code is 200 OK");
        }
    }
}