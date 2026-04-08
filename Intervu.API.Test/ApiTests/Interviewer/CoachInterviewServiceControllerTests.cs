using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.User;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Interviewer
{
    public class CoachInterviewServiceControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CoachInterviewServiceControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginCoachAsync()
        {
            var email = $"coach_service_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            var fullName = "Test Coach Service";

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = fullName,
                Role = "Coach"
            });

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        // Seeded Interview Type ID from IntervuPostgreDbContext
        private readonly Guid _cvInterviewTypeId = Guid.Parse("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa");

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task CoachInterviewService_Lifecycle_ReturnsSuccess()
        {
            // Arrange
            var (token, coachId) = await RegisterAndLoginCoachAsync();

            // 1. Create Service
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };

            LogInfo("Creating coach interview service.");
            var createResponse = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");
            var createResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(createResponse);
            var serviceId = createResult.Data!.Id;

            // 2. Get My Services
            LogInfo("Getting my interview services.");
            var getMineResponse = await _api.GetAsync("/api/v1/coach-interview-services/mine", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getMineResponse.StatusCode, "Get mine status code is 200 OK");
            var getMineResult = await _api.LogDeserializeJson<List<CoachInterviewServiceDto>>(getMineResponse);
            await AssertHelper.AssertTrue(getMineResult.Data!.Any(s => s.Id == serviceId), "Service is found in my services list");

            // 3. Get By Coach ID (Public)
            LogInfo($"Getting services for coach {coachId}.");
            var getByCoachResponse = await _api.GetAsync($"/api/v1/coach-interview-services/coach/{coachId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getByCoachResponse.StatusCode, "Get by coach ID status code is 200 OK");

            // 4. Update Service
            var updateDto = new UpdateCoachInterviewServiceDto
            {
                Price = 1800,
                DurationMinutes = 50
            };
            LogInfo($"Updating service {serviceId}.");
            var updateResponse = await _api.PutAsync($"/api/v1/coach-interview-services/{serviceId}", updateDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");

            // 5. Delete Service
            LogInfo($"Deleting service {serviceId}.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/coach-interview-services/{serviceId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task GetMyServices_ReturnsUnauthorized_WhenNoToken()
        {
            // Act
            LogInfo("Getting my services without token.");
            var response = await _api.GetAsync("/api/v1/coach-interview-services/mine", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }
    }
}