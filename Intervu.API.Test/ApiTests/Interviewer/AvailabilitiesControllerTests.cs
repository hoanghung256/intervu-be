using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Interviewer
{
    // TODO: sync to exits case
    public class AvailabilitiesControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public AvailabilitiesControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> CreateCoachProfileByAdminAndLoginCoachAsync()
        {
            var email = $"coach_avail_admin_{Guid.NewGuid():N}@example.com";
            var password = CANDIDATE_PASSWORD;
            var fullName = "Coach Avail Tester";

            var adminLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = ADMIN_EMAIL,
                Password = DEFAULT_PASSWORD
            }, logBody: true);
            var adminLoginData = await _api.LogDeserializeJson<LoginResponse>(adminLoginResponse);
            var adminToken = adminLoginData.Data!.Token;

            await _api.PostAsync("/api/v1/coach-profile", new CoachCreateDto
            {
                FullName = fullName,
                Email = email,
                Password = password,
                Role = UserRole.Coach,
                ExperienceYears = 1,
                CurrentAmount = 0
            }, jwtToken: adminToken, logBody: true);

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachAvailability")]
        public async Task CreateAvailability_ReturnsSuccess_WhenDataIsValid()
        {
            var (token, coachId) = await CreateCoachProfileByAdminAndLoginCoachAsync();
            var startTime = DateTimeOffset.UtcNow.AddDays(1);
            var endTime = startTime.AddHours(2);

            var createDto = new CoachAvailabilityCreateDto
            {
                CoachId = coachId,
                RangeStartTime = startTime,
                RangeEndTime = endTime
            };

            var response = await _api.PostAsync("/api/v1/availabilities", createDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachAvailability")]
        public async Task CreateAvailability_ReturnsBadRequest_WhenEndTimeBeforeStartTime()
        {
            var (token, coachId) = await CreateCoachProfileByAdminAndLoginCoachAsync();
            var startTime = DateTimeOffset.UtcNow.AddDays(1);
            var endTime = startTime.AddHours(-1);

            var createDto = new CoachAvailabilityCreateDto
            {
                CoachId = coachId,
                RangeStartTime = startTime,
                RangeEndTime = endTime
            };

            var response = await _api.PostAsync("/api/v1/availabilities", createDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachAvailability")]
        public async Task GetAvailabilities_ReturnsInternalServerError_WhenInvalidMonth()
        {
            var (token, coachId) = await CreateCoachProfileByAdminAndLoginCoachAsync();
            var response = await _api.GetAsync($"/api/v1/availabilities/{coachId}?month=13&year=2024", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Invalid month returns 500 Internal Server Error");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachAvailability")]
        public async Task DeleteAvailability_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var (token, _) = await CreateCoachProfileByAdminAndLoginCoachAsync();
            var nonExistentId = Guid.NewGuid();
            var response = await _api.DeleteAsync($"/api/v1/availabilities/{nonExistentId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent ID returns 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachAvailability")]
        public async Task UpdateAvailability_ReturnsBadRequest_WhenNoToken()
        {
            var updateDto = new CoachAvailabilityUpdateDto { CoachId = Guid.NewGuid() };
            var response = await _api.PutAsync("/api/v1/availabilities", updateDto, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "No token returns 400 Bad Request");
        }
    }
}
