using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Intervu.Application.DTOs.InterviewRoom;
using Xunit.Abstractions;
using Intervu.Application.DTOs.Common;
using Intervu.Domain.Entities.Constants;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.InterviewBooking;

namespace Intervu.API.Test.ApiTests.InterviewRoom
{
    public class InterviewRoomControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public InterviewRoomControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> LoginSeededUserAsync(string email, string role = "Candidate")
        {
            var password = role == "Admin" || role == "Coach" || email.Contains("alice") ? DEFAULT_PASSWORD : CANDIDATE_PASSWORD;
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
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

        private async Task<(string coachToken, Guid coachId, Guid serviceId, Guid availabilityId, DateTime startTime)> CreateCoachAndServiceAsync()
        {
            var adminToken = await LoginAdminAsync();

            // 1. Register and Login a new Coach
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

            // 2. Create Interview Type (using admin token)
            var interviewType = new InterviewTypeDto
            {
                Name = $"Test Type {Guid.NewGuid()}",
                Description = "Test Description",
                MinPrice = 0, // Set price to 0 to avoid payment webhook
                MaxPrice = 100,
                SuggestedDurationMinutes = 60,
                Status = InterviewTypeStatus.Active
            };
            var itResponse = await _api.PostAsync("/api/v1/InterviewType", interviewType, jwtToken: adminToken);
            var itResult = await _api.LogDeserializeJson<InterviewTypeDto>(itResponse);
            var itId = itResult.Data!.Id;

            // 3. Create Coach Interview Service (using coach token)
            var serviceDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = itId,
                Price = 0, // Set price to 0 for direct room creation
                DurationMinutes = 60
            };
            var serviceResponse = await _api.PostAsync("/api/v1/coach-interview-services", serviceDto, jwtToken: coachToken);
            var serviceResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(serviceResponse);
            var serviceId = serviceResult.Data!.Id;

            // 4. Create Coach Availability (using coach token)
            var startTime = DateTime.UtcNow.AddDays(7).AddHours(1);
            startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0, DateTimeKind.Utc); // Round to hour
            var endTime = startTime.AddHours(1); // 1-hour slot
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

            return (coachToken, coachId, serviceId, availabilityId, startTime);
        }

        private async Task<(Guid roomId, string candidateToken, Guid candidateId, string coachToken, Guid coachId)> CreateTestInterviewRoomAsync()
        {
            // 1. Register and Login a new Candidate
            var candidateEmail = $"candidate_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = candidateEmail,
                Password = CANDIDATE_PASSWORD,
                FullName = "Test Candidate",
                Role = "Candidate"
            });
            var candidateLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = candidateEmail, Password = CANDIDATE_PASSWORD });
            var candidateLoginData = await _api.LogDeserializeJson<LoginResponse>(candidateLoginResponse);
            var candidateToken = candidateLoginData.Data!.Token;
            var candidateId = candidateLoginData.Data.User.Id;

            // 2. Create Coach, Service, and Availability
            var (coachToken, coachId, serviceId, availabilityId, startTime) = await CreateCoachAndServiceAsync();

            // 3. Create Booking Request (this should create the InterviewRoom since price is 0)
            var bookingRequest = new InterviewBookingRequest
            {
                CoachId = coachId,
                CoachAvailabilityId = availabilityId,
                CoachInterviewServiceId = serviceId,
                StartTime = startTime,
                ReturnUrl = "https://test.com/return"
            };

            var bookingResponse = await _api.PostAsync("/api/v1/interview-booking", bookingRequest, jwtToken: candidateToken);
            var bookingResult = await _api.LogDeserializeJson<CreateBookingResponseData>(bookingResponse);

            // The room ID is not directly returned by the booking endpoint.
            // We need to fetch the candidate's rooms and find the newly created one.
            var roomsResponse = await _api.GetAsync("/api/v1/interviewroom?page=1&pageSize=10", jwtToken: candidateToken);
            var roomsResult = await _api.LogDeserializeJson<PagedResult<InterviewRoomDto>>(roomsResponse);
            var roomId = roomsResult.Data!.Items.First(r => r.CandidateId == candidateId && r.CoachId == coachId && r.ScheduledTime == startTime).Id;

            return (roomId, candidateToken, candidateId, coachToken, coachId);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetList_ReturnsSuccess_WhenUserIsAuthenticated()
        {
            // Arrange
            var (roomId, candidateToken, candidateId, coachToken, coachId) = await CreateTestInterviewRoomAsync();

            // Act
            LogInfo($"Getting interview room list for candidate {candidateId}.");
            var response = await _api.GetAsync("/api/v1/interviewroom?page=1&pageSize=10", jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<InterviewRoomDto>>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotEmpty(apiResponse.Data!.Items, "Candidate should have at least one room");
            await AssertHelper.AssertTrue(apiResponse.Data.Items.Any(r => r.Id == roomId), "Newly created room is in the list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetById_ReturnsSuccess_WhenRoomExists()
        {
            // Arrange
            var (roomId, candidateToken, _, _, _) = await CreateTestInterviewRoomAsync();

            // Act
            LogInfo($"Getting interview room {roomId} by ID as candidate.");
            var response = await _api.GetAsync($"/api/v1/interviewroom/{roomId}", jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<InterviewRoomDto>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertEqual(roomId, apiResponse.Data!.Id, "Returned room ID matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ManageCoachEvaluation_ReturnsSuccess()
        {
            // Arrange
            var (roomId, _, _, coachToken, _) = await CreateTestInterviewRoomAsync();
            // To test evaluation, the room needs to be in an 'Ongoing' or 'Completed' state.
            // There's no direct API to change room status. For now, we'll assume the room is created in a state
            // that allows evaluation, or mock the state if direct manipulation is not feasible via API.
            // For simplicity, let's assume the room is in a state where evaluation can be performed.

            // 1. Get Evaluation Form
            LogInfo($"Getting coach evaluation for room {roomId}.");
            var getResponse = await _api.GetAsync($"/api/v1/interviewroom/{roomId}/coach-evaluation", jwtToken: coachToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getResponse.StatusCode, "Get evaluation status code is 200 OK");

            // 2. Save Draft
            var evaluationRequest = new SubmitCoachEvaluationRequest
            {
                Results = new List<EvaluationResultDto>
                {
                    new EvaluationResultDto { Score = 5, Question = "Technical Skills?", Answer = "Very strong", Type = "Technical" }
                }
            };

            LogInfo("Saving evaluation draft.");
            var draftResponse = await _api.PatchAsync($"/api/v1/interviewroom/{roomId}/coach-evaluation/draft", evaluationRequest, jwtToken: coachToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, draftResponse.StatusCode, "Draft save status code is 200 OK");

            // 3. Submit Evaluation
            LogInfo("Submitting evaluation.");
            var submitResponse = await _api.PostAsync($"/api/v1/interviewroom/{roomId}/coach-evaluation", evaluationRequest, jwtToken: coachToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, submitResponse.StatusCode, "Submit status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ManageInterviewReport_ReturnsSuccess()
        {
            // Arrange
            var (roomId, candidateToken, _, _, _) = await CreateTestInterviewRoomAsync();
            var adminToken = await LoginAdminAsync();

            // 1. Report Problem
            var reportRequest = new CreateRoomReportRequest
            {
                Reason = "Technical Issue",
                Details = "Camera not working during the interview"
            };

            LogInfo($"Reporting problem for room {roomId} as candidate.");
            var reportResponse = await _api.PostAsync($"/api/v1/interviewroom/{roomId}/report", reportRequest, jwtToken: candidateToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, reportResponse.StatusCode, "Report status code is 200 OK");

            // 2. Get My Reports
            LogInfo("Getting candidate's own reports.");
            var myReportsResponse = await _api.GetAsync("/api/v1/interviewroom/my-reports", jwtToken: candidateToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, myReportsResponse.StatusCode, "Get my reports status code is 200 OK");

            // 3. Get All Reports (Admin)
            LogInfo("Admin getting all interview reports.");
            var allReportsResponse = await _api.GetAsync("/api/v1/admin/room-reports", jwtToken: adminToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, allReportsResponse.StatusCode, "Admin get reports status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ReportInterviewProblemLegacy_ReturnsSuccess()
        {
            // Arrange
            var (roomId, candidateToken, _, _, _) = await CreateTestInterviewRoomAsync();

            var legacyRequest = new CreateRoomReportLegacyRequest
            {
                InterviewRoomId = roomId,
                Reason = "Legacy Reason",
                Details = "Details for legacy report"
            };

            // Act
            LogInfo("Reporting problem via legacy endpoint.");
            var response = await _api.PostAsync("/api/v1/interviewroom/report", legacyRequest, jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Legacy report status code is 200 OK");
        }

        private class CreateAvailResponseData
        {
            public List<Guid> Ids { get; set; } = new();
            public int BlockCount { get; set; }
        }

        private class CreateBookingResponseData
        {
            public bool IsPaid { get; set; }
            public string? CheckOutUrl { get; set; }
        }
    }
}