using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Interviewer
{
    public class ViewInterviewServicesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _validInterviewTypeId = Guid.Parse("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa");

        public ViewInterviewServicesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginCoachAsync(string emailPrefix)
        {
            var email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com";
            var password = CANDIDATE_PASSWORD;

            var adminLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = ADMIN_EMAIL,
                Password = DEFAULT_PASSWORD
            }, logBody: true);
            var adminData = await _api.LogDeserializeJson<LoginResponse>(adminLogin);

            await _api.PostAsync("/api/v1/coach-profile", new CoachCreateDto
            {
                FullName = "View Service Coach",
                Email = email,
                Password = password,
                Role = UserRole.Coach,
                ExperienceYears = 4,
                CurrentAmount = 0
            }, jwtToken: adminData.Data!.Token, logBody: true);

            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password }, logBody: true);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(login);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<string> RegisterAndLoginCandidateAsync(string emailPrefix)
        {
            var email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com";
            var password = CANDIDATE_PASSWORD;

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                FullName = "View Service Candidate",
                Email = email,
                Password = password,
                Role = "Candidate"
            }, logBody: true);

            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password }, logBody: true);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(login);
            return loginData.Data!.Token;
        }

        private async Task<Guid> CreateServiceAsync(string coachToken)
        {
            var createResponse = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _validInterviewTypeId,
                Price = 1700,
                DurationMinutes = 75
            }, jwtToken: coachToken, logBody: true);

            var createData = await _api.LogDeserializeJson<CoachInterviewServiceDto>(createResponse, true);
            return createData.Data!.Id;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task ViewInterviewServices_Mine_ReturnsSuccess_WhenCoachHasServices()
        {
            var (token, _) = await RegisterAndLoginCoachAsync("view_service_mine");
            _ = await CreateServiceAsync(token);

            var response = await _api.GetAsync("/api/v1/coach-interview-services/mine", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Coach can view own services with 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task ViewInterviewServices_ByCoachId_ReturnsSuccess_WhenCoachExists()
        {
            var (token, coachId) = await RegisterAndLoginCoachAsync("view_service_by_coach");
            _ = await CreateServiceAsync(token);

            var response = await _api.GetAsync($"/api/v1/coach-interview-services/coach/{coachId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Public by-coach view returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task ViewInterviewServices_Mine_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync("/api/v1/coach-interview-services/mine", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Missing token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task ViewInterviewServices_Mine_WhenCandidateRole_ReturnsForbidden()
        {
            var candidateToken = await RegisterAndLoginCandidateAsync("view_service_candidate");

            var response = await _api.GetAsync("/api/v1/coach-interview-services/mine", jwtToken: candidateToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Candidate role returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task ViewInterviewServices_ByNonExistentCoach_ReturnsNotFound()
        {
            var response = await _api.GetAsync($"/api/v1/coach-interview-services/coach/{Guid.NewGuid()}", logBody: true);

            await AssertHelper.AssertNotEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent coach returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task ViewInterviewServices_Mine_WhenNoServices_ReturnsNotFound()
        {
            var (token, _) = await RegisterAndLoginCoachAsync("view_service_empty_mine");

            var response = await _api.GetAsync("/api/v1/coach-interview-services/mine", jwtToken: token, logBody: true);

            await AssertHelper.AssertNotEqual(HttpStatusCode.NotFound, response.StatusCode, "Coach with no services returns 404 NotFound");
        }
    }
}

