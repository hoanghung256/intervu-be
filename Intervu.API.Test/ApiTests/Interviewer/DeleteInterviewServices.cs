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
    public class DeleteInterviewServicesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _validInterviewTypeId = Guid.Parse("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa");

        public DeleteInterviewServicesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
                FullName = "Delete Service Coach",
                Email = email,
                Password = password,
                Role = UserRole.Coach,
                ExperienceYears = 3,
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
                FullName = "Delete Service Candidate",
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
                Price = 1600,
                DurationMinutes = 60
            }, jwtToken: coachToken, logBody: true);

            var createData = await _api.LogDeserializeJson<CoachInterviewServiceDto>(createResponse, true);
            return createData.Data!.Id;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteInterviewServices_ReturnsSuccess_WhenOwnerDeletesService()
        {
            var token = await RegisterAndLoginCoachAsync("delete_service_owner");
            var serviceId = await CreateServiceAsync(token);

            var response = await _api.DeleteAsync($"/api/v1/coach-interview-services/{serviceId}", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Owner delete returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteInterviewServices_WithoutToken_ReturnsUnauthorized()
        {
            var token = await RegisterAndLoginCoachAsync("delete_service_no_token_owner");
            var serviceId = await CreateServiceAsync(token);

            var response = await _api.DeleteAsync($"/api/v1/coach-interview-services/{serviceId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Missing token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteInterviewServices_WhenCandidateRole_ReturnsForbidden()
        {
            var ownerToken = await RegisterAndLoginCoachAsync("delete_service_owner_for_candidate");
            var candidateToken = await RegisterAndLoginCandidateAsync("delete_service_candidate");
            var serviceId = await CreateServiceAsync(ownerToken);

            var response = await _api.DeleteAsync($"/api/v1/coach-interview-services/{serviceId}", jwtToken: candidateToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Candidate role returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteInterviewServices_WhenDifferentCoach_ReturnsForbidden()
        {
            var ownerToken = await RegisterAndLoginCoachAsync("delete_service_owner_a");
            var otherToken = await RegisterAndLoginCoachAsync("delete_service_owner_b");
            var serviceId = await CreateServiceAsync(ownerToken);

            var response = await _api.DeleteAsync($"/api/v1/coach-interview-services/{serviceId}", jwtToken: otherToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Different coach returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteInterviewServices_NonExistentService_ReturnsNotFound()
        {
            var token = await RegisterAndLoginCoachAsync("delete_service_not_found");

            var response = await _api.DeleteAsync($"/api/v1/coach-interview-services/{Guid.NewGuid()}", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent service returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteInterviewServices_InvalidServiceIdRoute_ReturnsNotFound()
        {
            var token = await RegisterAndLoginCoachAsync("delete_service_invalid_route");

            var response = await _api.DeleteAsync("/api/v1/coach-interview-services/not-a-guid", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Invalid serviceId route returns 404 NotFound");
        }
    }
}

