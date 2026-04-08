using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewBooking;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class JoinInterviewRoomTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public JoinInterviewRoomTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetById_ReturnsSuccess_WhenRoomExists()
        {
            var (roomId, candidateToken, _, _, _) = await CreateTestInterviewRoomAsync();
            var response = await _api.GetAsync($"/api/v1/interviewroom/{roomId}", jwtToken: candidateToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetList_ReturnsSuccess_WhenUserIsAuthenticated()
        {
            var (_, candidateToken, _, _, _) = await CreateTestInterviewRoomAsync();
            var response = await _api.GetAsync("/api/v1/interviewroom?page=1&pageSize=10", jwtToken: candidateToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<InterviewRoomDto>>(response, true);
            await AssertHelper.AssertNotEmpty(apiResponse.Data!.Items, "Candidate should have at least one room");
        }

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<(string coachToken, Guid coachId, Guid serviceId, Guid availabilityId, DateTime startTime)> CreateCoachAndServiceAsync()
        {
            var adminToken = await LoginAdminAsync();
            var coachEmail = $"coach_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = coachEmail, Password = CANDIDATE_PASSWORD, FullName = "Test Coach", Role = "Coach" });
            var coachLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = coachEmail, Password = CANDIDATE_PASSWORD });
            var coachLoginData = await _api.LogDeserializeJson<LoginResponse>(coachLoginResponse);
            var coachToken = coachLoginData.Data!.Token;
            var coachId = coachLoginData.Data.User.Id;

            var itResponse = await _api.PostAsync("/api/v1/InterviewType", new InterviewTypeDto
            {
                Name = $"Test Type {Guid.NewGuid()}",
                Description = "Test Description",
                MinPrice = 0,
                MaxPrice = 100,
                SuggestedDurationMinutes = 60,
                Status = InterviewTypeStatus.Active
            }, jwtToken: adminToken);
            var itResult = await _api.LogDeserializeJson<InterviewTypeDto>(itResponse);

            var serviceResponse = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto { InterviewTypeId = itResult.Data!.Id, Price = 0, DurationMinutes = 60 }, jwtToken: coachToken);
            var serviceResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(serviceResponse);

            var startTime = new DateTime(DateTime.UtcNow.AddDays(7).Year, DateTime.UtcNow.AddDays(7).Month, DateTime.UtcNow.AddDays(7).Day, DateTime.UtcNow.AddDays(7).Hour, 0, 0, DateTimeKind.Utc).AddHours(1);
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto { CoachId = coachId, RangeStartTime = startTime, RangeEndTime = startTime.AddHours(1) }, jwtToken: coachToken);
            var scheduleResponse = await _api.GetAsync($"/api/v1/availabilities/{coachId}?month={startTime.Month}&year={startTime.Year}", jwtToken: coachToken);
            var scheduleResult = await _api.LogDeserializeJson<CoachScheduleDto>(scheduleResponse);

            return (coachToken, coachId, serviceResult.Data!.Id, scheduleResult.Data!.FreeSlots.First().Id, startTime);
        }

        private async Task<(Guid roomId, string candidateToken, Guid candidateId, string coachToken, Guid coachId)> CreateTestInterviewRoomAsync()
        {
            var candidateEmail = $"candidate_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = candidateEmail, Password = CANDIDATE_PASSWORD, FullName = "Test Candidate", Role = "Candidate" });
            var candidateLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = candidateEmail, Password = CANDIDATE_PASSWORD });
            var candidateLoginData = await _api.LogDeserializeJson<LoginResponse>(candidateLoginResponse);
            var candidateToken = candidateLoginData.Data!.Token;
            var candidateId = candidateLoginData.Data.User.Id;

            var (coachToken, coachId, serviceId, availabilityId, startTime) = await CreateCoachAndServiceAsync();
            await _api.PostAsync("/api/v1/interview-booking", new InterviewBookingRequest
            {
                CoachId = coachId,
                CoachAvailabilityId = availabilityId,
                CoachInterviewServiceId = serviceId,
                StartTime = startTime,
                ReturnUrl = "https://test.com/return"
            }, jwtToken: candidateToken);

            var roomsResponse = await _api.GetAsync("/api/v1/interviewroom?page=1&pageSize=10", jwtToken: candidateToken);
            var roomsResult = await _api.LogDeserializeJson<PagedResult<InterviewRoomDto>>(roomsResponse);
            var roomId = roomsResult.Data!.Items.First(r => r.CandidateId == candidateId && r.CoachId == coachId).Id;
            return (roomId, candidateToken, candidateId, coachToken, coachId);
        }
    }
}
