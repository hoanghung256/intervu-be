using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CandidateProfileController
{
    public class CandidateProfileManagementTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CandidateProfileManagementTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
            }, logBody: true);

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);

            return (loginData.Data!.Token, loginData.Data.User.Id);
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
            var (token, userId) = await RegisterAndLoginCandidateAsync();

            var response = await _api.GetAsync($"/api/v1/candidate-profile/{userId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task UpdateCandidateProfile_ReturnsSuccess_WhenDataIsValid()
        {
            var (token, userId) = await RegisterAndLoginCandidateAsync();
            var updateDto = new CandidateUpdateDto
            {
                FullName = "Candidate Updated",
                Email = "updated@example.com",
                Bio = "Updated Bio for testing purposes.",
                PortfolioUrl = "https://updated-portfolio.example.com/test",
                CVUrl = "https://updated-cv.example.com/test.pdf"
            };

            var response = await _api.PutAsync($"/api/v1/candidate-profile/{userId}", updateDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetCandidateRating_ReturnsSuccess_WhenAuthenticated()
        {
            var (token, userId) = await RegisterAndLoginCandidateAsync();

            var response = await _api.GetAsync($"/api/v1/candidate-profile/{userId}/rating", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task UpdateCandidateStatus_ReturnsSuccess_WhenAdminIsAuthenticated()
        {
            var adminToken = await LoginAdminAsync();
            var (_, targetCandidateId) = await RegisterAndLoginCandidateAsync();

            var response = await _api.PutAsync($"/api/v1/candidate-profile/{targetCandidateId}/status", 0, jwtToken: adminToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task DeleteCandidateProfile_ReturnsSuccess_WhenAdminIsAuthenticated()
        {
            var adminToken = await LoginAdminAsync();
            var (_, targetCandidateId) = await RegisterAndLoginCandidateAsync();

            var response = await _api.DeleteAsync($"/api/v1/candidate-profile/{targetCandidateId}", jwtToken: adminToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task ManageWorkExperiences_ReturnsSuccess()
        {
            var (token, userId) = await RegisterAndLoginCandidateAsync();

            var createDto = new CandidateWorkExperienceDto
            {
                CompanyName = "Initial Corp",
                PositionTitle = "Junior dev",
                StartDate = DateTime.UtcNow.AddYears(-1),
                IsCurrentWorking = true
            };

            var createResponse = await _api.PostAsync($"/api/v1/candidate-profile/{userId}/work-experiences", createDto, jwtToken: token, logBody: true);
            var createResult = await _api.LogDeserializeJson<CandidateWorkExperienceDto>(createResponse);
            var workId = createResult.Data!.Id;

            var updateDto = new CandidateWorkExperienceDto
            {
                Id = workId,
                CompanyName = "Updated Corp",
                PositionTitle = "Senior dev",
                StartDate = DateTime.UtcNow.AddYears(-1),
                IsCurrentWorking = true
            };

            var updateResponse = await _api.PutAsync($"/api/v1/candidate-profile/{userId}/work-experiences/{workId}", updateDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");

            var batchResponse = await _api.PutAsync($"/api/v1/candidate-profile/{userId}/work-experiences", new UpdateCandidateWorkExperiencesRequest
            {
                WorkExperiences = new List<CandidateWorkExperienceDto> { updateDto }
            }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, batchResponse.StatusCode, "Batch update status code is 200 OK");

            var deleteResponse = await _api.DeleteAsync($"/api/v1/candidate-profile/{userId}/work-experiences/{workId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task ManageCertificates_ReturnsSuccess()
        {
            var (token, userId) = await RegisterAndLoginCandidateAsync();

            var addResponse = await _api.PostAsync($"/api/v1/candidate-profile/{userId}/certificates", new CandidateCertificateDto
            {
                Name = "AWS Cloud Practitioner",
                Issuer = "Amazon Web Services",
                IssuedAt = DateTime.UtcNow.AddMonths(-3)
            }, jwtToken: token, logBody: true);

            var addResult = await _api.LogDeserializeJson<CandidateCertificateDto>(addResponse);
            var certId = addResult.Data!.Id;

            var updateResponse = await _api.PutAsync($"/api/v1/candidate-profile/{userId}/certificates/{certId}", new CandidateCertificateDto
            {
                Id = certId,
                Name = "AWS Certified Developer",
                Issuer = "Amazon Web Services",
                IssuedAt = DateTime.UtcNow.AddMonths(-3)
            }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update certificate status code is 200 OK");

            var deleteResponse = await _api.DeleteAsync($"/api/v1/candidate-profile/{userId}/certificates/{certId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete certificate status code is 200 OK");
        }
    }
}
