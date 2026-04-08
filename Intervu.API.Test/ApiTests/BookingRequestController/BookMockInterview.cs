using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.BookingRequestController
{
    public class BookMockInterviewTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        public BookMockInterviewTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CreateJDBookingRequest_SingleRound_ReturnsSuccess()
        {
            var token = await LoginSeededCandidateAsync();
            var services = await GetCoachServicesAsync();
            var service1 = services.First();
            var requiredBlocks = GetRequiredBlockCount(service1.DurationMinutes);
            var allBlocks = await CreateAvailabilityBlocksAsync(requiredBlocks, 20, 9);

            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/jd-single.pdf",
                CVUrl = "https://example.com/cv-single.pdf",
                AimLevel = AimLevel.MidLevel,
                Rounds =
                [
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service1.Id,
                        AvailabilityIds = allBlocks.Take(requiredBlocks).ToList()
                    }
                ]
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Single round booking status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Single round booking succeeds");
            await AssertHelper.AssertEqual("JD multi-round booking request created successfully", payload.Message, "Success message matches");
            await AssertHelper.AssertNotNull(payload.Data, "Booking request data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CreateJDBookingRequest_MultipleRounds_ReturnsSuccess()
        {
            var token = await LoginSeededCandidateAsync();
            var services = await GetCoachServicesAsync();
            var service1 = services[0];
            var service2 = services.Count > 1 ? services[1] : services[0];
            var required1 = GetRequiredBlockCount(service1.DurationMinutes);
            var required2 = GetRequiredBlockCount(service2.DurationMinutes);
            var allBlocks = await CreateAvailabilityBlocksAsync(required1 + required2, 21, 11);

            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/jd-multi.pdf",
                CVUrl = "https://example.com/cv-multi.pdf",
                AimLevel = AimLevel.Senior,
                Rounds =
                [
                    new CreateInterviewRoundDto { CoachInterviewServiceId = service1.Id, AvailabilityIds = allBlocks.Take(required1).ToList() },
                    new CreateInterviewRoundDto { CoachInterviewServiceId = service2.Id, AvailabilityIds = allBlocks.Skip(required1).Take(required2).ToList() }
                ]
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Multi-round booking status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Multi-round booking succeeds");
            await AssertHelper.AssertEqual("JD multi-round booking request created successfully", payload.Message, "Success message matches");
            await AssertHelper.AssertNotNull(payload.Data, "Booking request data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CreateJDBookingRequest_WithoutToken_ReturnsUnauthorized()
        {
            var services = await GetCoachServicesAsync();
            var service = services.First();
            var requiredBlocks = GetRequiredBlockCount(service.DurationMinutes);
            var availabilityIds = await CreateAvailabilityBlocksAsync(requiredBlocks, 25, 9);

            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/no-token-jd.pdf",
                CVUrl = "https://example.com/no-token-cv.pdf",
                AimLevel = AimLevel.MidLevel,
                Rounds =
                [
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service.Id,
                        AvailabilityIds = availabilityIds
                    }
                ]
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CreateJDBookingRequest_WithEmptyRounds_ReturnsBadRequest()
        {
            // Arrange
            var token = await LoginSeededCandidateAsync();

            // Act – submit a booking with no rounds (violates MinLength(1) validation)
            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/jd-empty.pdf",
                CVUrl = "https://example.com/cv-empty.pdf",
                AimLevel = AimLevel.Junior,
                Rounds = []
            }, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Empty rounds list returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CreateJDBookingRequest_AsCoach_ReturnsForbidden()
        {
            // Arrange – coaches are not allowed to create booking requests (Candidate policy)
            var loginResponse = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = COACH_EMAIL, Password = DEFAULT_PASSWORD }, logBody: true);
            var loginPayload = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var coachToken = loginPayload.Data!.Token;

            var services = await GetCoachServicesAsync();
            var service = services.First();
            var requiredBlocks = GetRequiredBlockCount(service.DurationMinutes);
            var availabilityIds = await CreateAvailabilityBlocksAsync(requiredBlocks, 35, 10);

            // Act
            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/coach-jd.pdf",
                CVUrl = "https://example.com/coach-cv.pdf",
                AimLevel = AimLevel.MidLevel,
                Rounds =
                [
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service.Id,
                        AvailabilityIds = availabilityIds
                    }
                ]
            }, jwtToken: coachToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Coach role cannot create booking requests – 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CreateJDBookingRequest_WithInvalidJobDescriptionUrl_ReturnsBadRequest()
        {
            // Arrange – [Url] attribute rejects non-URL strings
            var token = await LoginSeededCandidateAsync();
            var services = await GetCoachServicesAsync();
            var service = services.First();
            var requiredBlocks = GetRequiredBlockCount(service.DurationMinutes);
            var availabilityIds = await CreateAvailabilityBlocksAsync(requiredBlocks, 40, 8);

            // Act
            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "not-a-valid-url",   // fails [Url] validation
                CVUrl = "https://example.com/cv.pdf",
                AimLevel = AimLevel.MidLevel,
                Rounds =
                [
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service.Id,
                        AvailabilityIds = availabilityIds
                    }
                ]
            }, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid JobDescriptionUrl returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CreateJDBookingRequest_WithAdminToken_ReturnsForbidden()
        {
            // Arrange – admin role is not in the Candidate authorization policy
            var loginResponse = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD }, logBody: true);
            var loginPayload = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var adminToken = loginPayload.Data!.Token;

            // Act – admin tries to create a booking (no availability setup needed; 403 fires at the auth layer)
            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/admin-jd.pdf",
                CVUrl = "https://example.com/admin-cv.pdf",
                AimLevel = AimLevel.Junior,
                Rounds = []
            }, jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Admin role cannot create booking requests – 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CreateJDBookingRequest_GuidEmptyCoachId_ThrowsException()
        {
            // Arrange – Guid.Empty passes [Required] model validation but the coach does not exist in the DB
            var token = await LoginSeededCandidateAsync();
            var services = await GetCoachServicesAsync();
            var service = services.First();
            var requiredBlocks = GetRequiredBlockCount(service.DurationMinutes);
            var availabilityIds = await CreateAvailabilityBlocksAsync(requiredBlocks, 43, 7);

            // Act & Assert – business logic throws when the coach cannot be found
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
                {
                    CoachId = Guid.Empty,
                    JobDescriptionUrl = "https://example.com/empty-coach-jd.pdf",
                    CVUrl = "https://example.com/empty-coach-cv.pdf",
                    AimLevel = AimLevel.Junior,
                    Rounds =
                    [
                        new CreateInterviewRoundDto
                        {
                            CoachInterviewServiceId = service.Id,
                            AvailabilityIds = availabilityIds
                        }
                    ]
                }, jwtToken: token, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "Exception is raised for Guid.Empty coach ID");
        }

        private async Task<string> LoginSeededCandidateAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD }, logBody: true);
            var payload = await _api.LogDeserializeJson<LoginResponse>(response, logBody: true);
            return payload.Data!.Token;
        }

        private async Task<List<CoachInterviewServiceDto>> GetCoachServicesAsync()
        {
            var response = await _api.GetAsync($"/api/v1/coach-interview-services/coach/{BobCoachId}", logBody: true);
            var payload = await _api.LogDeserializeJson<List<CoachInterviewServiceDto>>(response, logBody: true);
            return payload.Data!;
        }

        private async Task<List<Guid>> CreateAvailabilityBlocksAsync(int requiredBlocks, int dayOffset, int hourOffset)
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(dayOffset).Date.AddHours(hourOffset));
            var end = start.AddMinutes(requiredBlocks * 30);
            var createResponse = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);
            return createPayload.Data!.GetProperty("ids").EnumerateArray().Select(x => x.GetGuid()).ToList();
        }

        private static int GetRequiredBlockCount(int durationMinutes) => (durationMinutes + 29) / 30;

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
