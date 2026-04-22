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
    public class AddInterviewServicesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _validInterviewTypeId = Guid.Parse("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa");

        public AddInterviewServicesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> RegisterAndLoginCoachAsync(string emailPrefix)
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
                FullName = "Add Service Coach",
                Email = email,
                Password = password,
                Role = UserRole.Coach,
                ExperienceYears = 2,
                CurrentAmount = 0
            }, jwtToken: adminData.Data!.Token, logBody: true);

            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password }, logBody: true);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(login);
            return loginData.Data!.Token;
        }

        private async Task<string> RegisterAndLoginCandidateAsync(string emailPrefix)
        {
            var email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com";
            var password = CANDIDATE_PASSWORD;

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                FullName = "Candidate Service User",
                Email = email,
                Password = password,
                Role = "Candidate"
            }, logBody: true);

            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password }, logBody: true);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(login);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task AddInterviewServices_ReturnsSuccess_WhenCoachSendsValidRequest()
        {
            var token = await RegisterAndLoginCoachAsync("add_service_normal");

            var response = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _validInterviewTypeId,
                Price = 1200,
                DurationMinutes = 60
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Valid create request returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task AddInterviewServices_ZeroPrice_ReturnsBadRequest()
        {
            var token = await RegisterAndLoginCoachAsync("add_service_price_zero");

            var response = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _validInterviewTypeId,
                Price = 0,
                DurationMinutes = 60
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Price=0 returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task AddInterviewServices_NegativeDuration_ReturnsBadRequest()
        {
            var token = await RegisterAndLoginCoachAsync("add_service_duration_negative");

            var response = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _validInterviewTypeId,
                Price = 1200,
                DurationMinutes = -30
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Negative duration returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task AddInterviewServices_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _validInterviewTypeId,
                Price = 1200,
                DurationMinutes = 60
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Missing token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task AddInterviewServices_WhenCandidateRole_ReturnsForbidden()
        {
            var token = await RegisterAndLoginCandidateAsync("add_service_candidate");

            var response = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _validInterviewTypeId,
                Price = 1200,
                DurationMinutes = 60
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Candidate role returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task AddInterviewServices_InvalidInterviewType_ReturnsNotFound()
        {
            var token = await RegisterAndLoginCoachAsync("add_service_invalid_type");

            var response = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = Guid.NewGuid(),
                Price = 1200,
                DurationMinutes = 60
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Invalid interview type returns 404 NotFound");
        }
    }
}
