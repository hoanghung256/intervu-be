using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Interviewer
{
    public class AvailabilitiesControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public AvailabilitiesControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginCoachAsync()
        {
            var email = $"coach_avail_{Guid.NewGuid()}@example.com";
            var password = DEFAULT_PASSWORD;
            var fullName = "Coach Avail Tester";

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

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachAvailability")]
        public async Task CreateAvailability_ReturnsSuccess_WhenDataIsValid()
        {
            var (token, coachId) = await RegisterAndLoginCoachAsync();
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
            var (token, coachId) = await RegisterAndLoginCoachAsync();
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
        public async Task GetAvailabilities_ReturnsBadRequest_WhenInvalidMonth()
        {
            var (token, coachId) = await RegisterAndLoginCoachAsync();
            var response = await _api.GetAsync($"/api/v1/availabilities/{coachId}?month=13&year=2024", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid month returns 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachAvailability")]
        public async Task DeleteAvailability_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var (token, _) = await RegisterAndLoginCoachAsync();
            var nonExistentId = Guid.NewGuid();
            var response = await _api.DeleteAsync($"/api/v1/availabilities/{nonExistentId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent ID returns 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachAvailability")]
        public async Task UpdateAvailability_ReturnsUnauthorized_WhenNoToken()
        {
            var updateDto = new CoachAvailabilityUpdateDto { CoachId = Guid.NewGuid() };
            var response = await _api.PutAsync("/api/v1/availabilities", updateDto, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token returns 401 Unauthorized");
        }
    }
}
