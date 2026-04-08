using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewBooking;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Domain.Entities.Constants;
using Intervu.Application.DTOs.InterviewRoom;

namespace Intervu.API.Test.ApiTests.BookingRequest
{
    public class InterviewBookingControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public InterviewBookingControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> LoginAsAliceAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<string> LoginAdminAsync()
        {
            var loginRequest = new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD };
            var response = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<(Guid coachId, Guid availabilityId, Guid serviceId, DateTime startTime)> SetupTestData()
        {
            var adminToken = await LoginAdminAsync();

            // 1. Create a Coach
            var coachEmail = $"coach_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = coachEmail,
                Password = CANDIDATE_PASSWORD,
                FullName = "Test Coach",
                Role = "Coach"
            });

            var coachLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = coachEmail, Password = CANDIDATE_PASSWORD });
            var coachLoginData = await _api.LogDeserializeJson<LoginResponse>(coachLoginResponse);
            var coachToken = coachLoginData.Data!.Token;
            var coachId = coachLoginData.Data.User.Id;

            // 2. Create Interview Type
            var interviewType = new InterviewTypeDto
            {
                Name = $"Test Type {Guid.NewGuid()}",
                Description = "Test Description",
                MinPrice = 1000,
                MaxPrice = 5000,
                SuggestedDurationMinutes = 60,
                Status = InterviewTypeStatus.Active
            };
            var itResponse = await _api.PostAsync("/api/v1/InterviewType", interviewType, jwtToken: adminToken);
            var itResult = await _api.LogDeserializeJson<InterviewTypeDto>(itResponse);
            var itId = itResult.Data!.Id;

            // 3. Create Coach Interview Service
            var serviceDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = itId,
                Price = 2000,
                DurationMinutes = 60
            };
            var serviceResponse = await _api.PostAsync("/api/v1/coach-interview-services", serviceDto, jwtToken: coachToken);
            var serviceResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(serviceResponse);
            var serviceId = serviceResult.Data!.Id;

            // 4. Create Coach Availability
            var startTime = DateTime.UtcNow.AddDays(7).AddHours(1);
            startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0, DateTimeKind.Utc);
            var endTime = startTime.AddHours(2);
            var availDto = new CoachAvailabilityCreateDto
            {
                CoachId = coachId,
                RangeStartTime = startTime,
                RangeEndTime = endTime
            };
            var availResponse = await _api.PostAsync("/api/v1/availabilities", availDto, jwtToken: coachToken);
            var availResult = await _api.LogDeserializeJson<CreateAvailResponseData>(availResponse);

            // To get availabilityId, we fetch coach schedule
            var scheduleResponse = await _api.GetAsync($"/api/v1/availabilities/{coachId}?month={startTime.Month}&year={startTime.Year}", jwtToken: coachToken);
            var scheduleResult = await _api.LogDeserializeJson<CoachScheduleDto>(scheduleResponse);
            var availabilityId = scheduleResult.Data!.FreeSlots.First().Id;

            return (coachId, availabilityId, serviceId, startTime);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CreatePaymentUrl_ReturnsSuccess()
        {
            // Arrange
            var (token, _) = await LoginAsAliceAsync();
            var (coachId, availabilityId, serviceId, startTime) = await SetupTestData();

            var bookingRequest = new InterviewBookingRequest
            {
                CoachId = coachId,
                CoachAvailabilityId = availabilityId,
                CoachInterviewServiceId = serviceId,
                StartTime = startTime,
                ReturnUrl = "https://test.com/return"
            };

            // Act
            LogInfo("Creating booking payment URL.");
            var response = await _api.PostAsync("/api/v1/interview-booking", bookingRequest, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CreateBookingResponseData>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetHistory_ReturnsSuccess()
        {
            // Arrange
            var (token, _) = await LoginAsAliceAsync();

            // Act
            LogInfo("Getting booking history.");
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

//         [Fact]
//         [Trait("Category", "API")]
//         [Trait("Category", "InterviewBooking")]
//         public async Task CancelInterview_ReturnsSuccess()
//         {
//             // Arrange
//             var (token, _) = await LoginAsAliceAsync();
//
//             // To cancel an interview, we first need to have a booked interview room.
//             // Since we don't have a direct "Create Interview Room" API for candidates (it's created via booking),
//             // and completing a booking requires a payment webhook, we might need to rely on seeded data
//             // OR find another way to create a room.
//
//             // Looking at IntervuPostgreDbContext, room1Id is status 'Scheduled'.
//             var room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");
//
//             // Act
//             LogInfo($"Cancelling interview room {room1Id}.");
//             var response = await _api.PostAsync($"/api/v1/interview-booking/cancel/{room1Id}", new { }, jwtToken: token, logBody: true);
//
//             // Assert
//             await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
//             var apiResponse = await _api.LogDeserializeJson<CancelBookingResponseData>(response);
//             await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
//         }

        private class CreateBookingResponseData
        {
            public bool IsPaid { get; set; }
            public string? CheckOutUrl { get; set; }
        }

        private class CancelBookingResponseData
        {
            public int RefundAmount { get; set; }
        }

        private class CreateAvailResponseData
        {
            public List<Guid> Ids { get; set; } = new();
            public int BlockCount { get; set; }
        }
    }
}