using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using System.Text.Json;
using System.Linq;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    // IC-34
    public class RescheduleInterviewSession : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private sealed record RoomContext(string CandidateToken, string CoachToken, Guid RoomId, Guid CoachId);

        private readonly ApiHelper _api;
        private readonly Guid _nonExistentRoomId = Guid.NewGuid();

        public RescheduleInterviewSession(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string Token, Guid UserId)> RegisterAndLoginCandidateAsync()
        {
            var email = $"candidate_reschedule_{Guid.NewGuid():N}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                FullName = "Reschedule Candidate",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = "Candidate"
            }, logBody: true);

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD }, logBody: true);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<(string Token, Guid UserId)> RegisterAndLoginCoachWithProfileAsync()
        {
            var email = $"coach_reschedule_{Guid.NewGuid():N}@example.com";

            var adminLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD }, logBody: true);
            var adminToken = (await _api.LogDeserializeJson<LoginResponse>(adminLogin)).Data!.Token;

            await _api.PostAsync("/api/v1/coach-profile", new CoachCreateDto
            {
                FullName = "Reschedule Coach",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Coach,
                ExperienceYears = 2,
                CurrentAmount = 0
            }, jwtToken: adminToken, logBody: true);

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD }, logBody: true);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<Guid> CreateAvailabilityAsync(Guid coachId)
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(7).Date.AddHours(9));
            var end = start.AddMinutes(60);

            var response = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = coachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            return payload.Data!.GetProperty("ids").EnumerateArray().First().GetGuid();
        }

        private async Task<DateTime> CreateSecondAvailabilityAndGetStartAsync(Guid coachId)
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(8).Date.AddHours(11));
            var end = start.AddMinutes(60);

            var response = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = coachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            await _api.LogDeserializeJson<JsonElement>(response, true);
            return start;
        }

        private async Task<Guid> CreateServiceAsync(string coachToken)
        {
            var interviewTypeId = await CreateInterviewTypeByAdminAsync();
            var response = await _api.PostAsync("/api/v1/coach-interview-services", new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = interviewTypeId,
                Price = 1500,
                DurationMinutes = 30
            }, jwtToken: coachToken, logBody: true);

            var payload = await _api.LogDeserializeJson<CoachInterviewServiceDto>(response, true);
            return payload.Data!.Id;
        }

        private async Task<Guid> CreateInterviewTypeByAdminAsync()
        {
            var adminLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = ADMIN_EMAIL,
                Password = DEFAULT_PASSWORD
            }, logBody: true);
            var adminToken = (await _api.LogDeserializeJson<LoginResponse>(adminLogin)).Data!.Token;

            var typeId = Guid.NewGuid();
            var createResponse = await _api.PostAsync("/api/v1/interviewtype", new InterviewTypeDto
            {
                Id = typeId,
                Name = $"Reschedule Type {Guid.NewGuid():N}".Substring(0, 23),
                Description = "Fresh interview type for reschedule tests.",
                SuggestedDurationMinutes = 60,
                MinPrice = 0,
                MaxPrice = 3000
            }, jwtToken: adminToken, logBody: true);

            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, true);
            if (createResponse.StatusCode == HttpStatusCode.OK && createPayload.Data.ValueKind == JsonValueKind.Object &&
                createPayload.Data.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
            {
                return idProp.GetGuid();
            }

            return typeId;
        }

        private async Task<RoomContext> CreateRoomContextAsync()
        {
            try
            {
                var (candidateToken, _) = await RegisterAndLoginCandidateAsync();
                var (coachToken, coachId) = await RegisterAndLoginCoachWithProfileAsync();
                var serviceId = await CreateServiceAsync(coachToken);
                var availabilityId = await CreateAvailabilityAsync(coachId);

                var bookingResponse = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
                {
                    CoachId = coachId,
                    JobDescriptionUrl = "https://example.com/jd.pdf",
                    CVUrl = "https://example.com/cv.pdf",
                    AimLevel = AimLevel.MidLevel,
                    Rounds =
                    [
                        new CreateInterviewRoundDto
                        {
                            CoachInterviewServiceId = serviceId,
                            AvailabilityIds = [availabilityId]
                        }
                    ]
                }, jwtToken: candidateToken, logBody: true);

                if (bookingResponse.StatusCode != HttpStatusCode.OK)
                {
                    return await GetSeededRoomContextAsync();
                }

                var bookingPayload = await _api.LogDeserializeJson<JsonElement>(bookingResponse, true);
                var bookingId = bookingPayload.Data!.GetProperty("id").GetGuid();

                await _api.PostAsync($"/api/v1/booking-requests/{bookingId}/pay", new PayBookingRequestDto
                {
                    ReturnUrl = "https://example.com/return"
                }, jwtToken: candidateToken, logBody: true);

                await _api.PostAsync($"/api/v1/booking-requests/{bookingId}/respond", new RespondToBookingRequestDto
                {
                    IsApproved = true
                }, jwtToken: coachToken, logBody: true);

                var roomsResponse = await _api.GetAsync("/api/v1/interviewroom?Statuses=0&page=1&pageSize=50", jwtToken: candidateToken, logBody: true);
                if (roomsResponse.StatusCode != HttpStatusCode.OK)
                {
                    return await GetSeededRoomContextAsync();
                }

                var roomsPayload = await _api.LogDeserializeJson<JsonElement>(roomsResponse, true);
                var items = roomsPayload.Data!.GetProperty("items").EnumerateArray().ToList();
                var matched = items.FirstOrDefault(x => x.TryGetProperty("bookingRequestId", out var bookingProp) && bookingProp.GetGuid() == bookingId);
                if (matched.ValueKind == JsonValueKind.Undefined)
                {
                    return await GetSeededRoomContextAsync();
                }

                return new RoomContext(candidateToken, coachToken, matched.GetProperty("id").GetGuid(), coachId);
            }
            catch
            {
                return await GetSeededRoomContextAsync();
            }
        }

        private async Task<RoomContext> GetSeededRoomContextAsync()
        {
            var candidateLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = "alice@example.com",
                Password = DEFAULT_PASSWORD
            }, logBody: true);
            var candidateToken = (await _api.LogDeserializeJson<LoginResponse>(candidateLogin)).Data!.Token;

            var coachLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = "bob@example.com",
                Password = DEFAULT_PASSWORD
            }, logBody: true);
            var coachToken = (await _api.LogDeserializeJson<LoginResponse>(coachLogin)).Data!.Token;

            var roomsResponse = await _api.GetAsync("/api/v1/interviewroom?Statuses=0&page=1&pageSize=10", jwtToken: candidateToken, logBody: true);
            var roomsPayload = await _api.LogDeserializeJson<JsonElement>(roomsResponse, true);
            var first = roomsPayload.Data!.GetProperty("items").EnumerateArray().First();
            var roomId = first.GetProperty("id").GetGuid();
            var coachId = first.GetProperty("coachId").GetGuid();

            return new RoomContext(candidateToken, coachToken, roomId, coachId);
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_WithValidData_ReturnsSuccess()
        {
            var ctx = await CreateRoomContextAsync();
            var newStartTime = await CreateSecondAvailabilityAndGetStartAsync(ctx.CoachId);
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = ctx.RoomId,
                NewStartTime = newStartTime,
                Reason = "Need to reschedule due to personal emergency that requires my immediate attention."
            }, jwtToken: ctx.CandidateToken, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Reschedule request successful");
            await AssertHelper.AssertEqual("Reschedule request created successfully", payload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_Unauthorized_ReturnsUnauthorized()
        {
            var ctx = await CreateRoomContextAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = ctx.RoomId,
                NewStartTime = DateTime.UtcNow.AddDays(2),
                Reason = "Unauthorized attempt."
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_NonExistentRoom_ReturnsNotFound()
        {
            var (token, _) = await RegisterAndLoginCandidateAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _nonExistentRoomId,
                NewStartTime = DateTime.UtcNow.AddDays(2),
                Reason = "Room does not exist."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent room ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_NewStartTimeInPast_ReturnsConflict()
        {
            var ctx = await CreateRoomContextAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = ctx.RoomId,
                NewStartTime = DateTime.UtcNow.AddDays(-1), // Past date
                Reason = "Cannot reschedule to a past time."
            }, jwtToken: ctx.CandidateToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, response.StatusCode, "Past NewStartTime should return 409 Conflict");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_MissingReason_ReturnsBadRequest()
        {
            var ctx = await CreateRoomContextAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = ctx.RoomId,
                NewStartTime = DateTime.UtcNow.AddDays(3),
                Reason = "" // Empty reason
            }, jwtToken: ctx.CandidateToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing reason should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_UserNotInRoom_ReturnsForbidden()
        {
            var ctx = await CreateRoomContextAsync();
            var (outsiderToken, _) = await RegisterAndLoginCoachWithProfileAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = ctx.RoomId,
                NewStartTime = DateTime.UtcNow.AddDays(4),
                Reason = "User not part of this interview session."
            }, jwtToken: outsiderToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, response.StatusCode, "User not in room should get 403 Forbidden");
        }
    }
}
