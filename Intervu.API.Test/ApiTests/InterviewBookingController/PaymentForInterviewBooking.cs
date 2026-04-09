using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.InterviewBooking;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;
using static Intervu.API.Test.Utils.ApiHelper;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class PaymentForInterviewBookingTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public PaymentForInterviewBookingTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CreatePaymentUrl_ReturnsSuccess()
        {
            var (token, _) = await LoginAsAliceAsync();
            var (coachId, availabilityId, serviceId, startTime) = await SetupTestData();

            var response = await _api.PostAsync("/api/v1/interview-booking", new InterviewBookingRequest
            {
                CoachId = coachId,
                CoachAvailabilityId = availabilityId,
                CoachInterviewServiceId = serviceId,
                StartTime = startTime,
                ReturnUrl = "https://test.com/return"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CreateBookingResponseData>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Data?.CheckOutUrl, "CheckOutUrl is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CreatePaymentUrl_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync("/api/v1/interview-booking", new InterviewBookingRequest
            {
                CoachId = Guid.NewGuid(),
                CoachAvailabilityId = Guid.NewGuid(),
                CoachInterviewServiceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(1),
                ReturnUrl = "https://test.com/return"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CreatePaymentUrl_InvalidData_ReturnsBadRequest()
        {
            var (token, _) = await LoginAsAliceAsync();
            var response = await _api.PostAsync("/api/v1/interview-booking", new InterviewBookingRequest
            {
                CoachId = Guid.Empty, // Invalid ID
                CoachAvailabilityId = Guid.NewGuid(),
                CoachInterviewServiceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(-1), // Past date
                ReturnUrl = "" // Missing URL
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid data");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CreatePaymentUrl_AlreadyBookedAvailability_ReturnsConflict()
        {
            var (token, _) = await LoginAsAliceAsync();
            var (coachId, availabilityId, serviceId, startTime) = await SetupTestData();

            // First booking
            await _api.PostAsync("/api/v1/interview-booking", new InterviewBookingRequest
            {
                CoachId = coachId,
                CoachAvailabilityId = availabilityId,
                CoachInterviewServiceId = serviceId,
                StartTime = startTime,
                ReturnUrl = "https://test.com/return"
            }, jwtToken: token, logBody: true);

            // Try second booking for same availability
            var response = await _api.PostAsync("/api/v1/interview-booking", new InterviewBookingRequest
            {
                CoachId = coachId,
                CoachAvailabilityId = availabilityId,
                CoachInterviewServiceId = serviceId,
                StartTime = startTime,
                ReturnUrl = "https://test.com/return"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, response.StatusCode, "Status code is 409 Conflict for already booked availability");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CreatePaymentUrl_NonExistentCoach_ReturnsNotFound()
        {
            var (token, _) = await LoginAsAliceAsync();
            var (_, availabilityId, serviceId, startTime) = await SetupTestData();

            var response = await _api.PostAsync("/api/v1/interview-booking", new InterviewBookingRequest
            {
                CoachId = Guid.NewGuid(), // Non-existent coach
                CoachAvailabilityId = availabilityId,
                CoachInterviewServiceId = serviceId,
                StartTime = startTime,
                ReturnUrl = "https://test.com/return"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found for non-existent coach");
        }

        private async Task<(string token, Guid userId)> LoginAsAliceAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<(Guid coachId, Guid availabilityId, Guid serviceId, DateTime startTime)> SetupTestData()
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
                MinPrice = 1000,
                MaxPrice = 5000,
                SuggestedDurationMinutes = 60,
                Status = InterviewTypeStatus.Active
            }, jwtToken: adminToken);
            var itResult = await _api.LogDeserializeJson<InterviewTypeDto>(itResponse);

            var serviceResponse = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = itResult.Data!.Id,
                Price = 2000,
                DurationMinutes = 60
            }, jwtToken: coachToken);
            var serviceResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(serviceResponse);

            var startTime = new DateTime(DateTime.UtcNow.AddDays(7).Year, DateTime.UtcNow.AddDays(7).Month, DateTime.UtcNow.AddDays(7).Day, DateTime.UtcNow.AddDays(7).Hour, 0, 0, DateTimeKind.Utc).AddHours(1);
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = coachId,
                RangeStartTime = startTime,
                RangeEndTime = startTime.AddHours(2)
            }, jwtToken: coachToken);
            var scheduleResponse = await _api.GetAsync($"/api/v1/availabilities/{coachId}?month={startTime.Month}&year={startTime.Year}", jwtToken: coachToken);
            var scheduleResult = await _api.LogDeserializeJson<CoachScheduleDto>(scheduleResponse);

            return (coachId, scheduleResult.Data!.FreeSlots.First().Id, serviceResult.Data!.Id, startTime);
        }

        private class CreateBookingResponseData : ApiResponse<CreateBookingData> {}
        private class CreateBookingData
        {
            public bool IsPaid { get; set; }
            public string? CheckOutUrl { get; set; }
        }
    }
}
