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
    public class CancelInterviewSessionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
        private static readonly Guid NonExistentBookingId = Guid.NewGuid();

        public CancelInterviewSessionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_SingleRound_ReturnsCancelledStatus()
        {
            var token = await LoginSeededCandidateAsync();
            var bookingId = await CreateBookingAndGetIdAsync(token, false, 22, 14);

            var cancelResponse = await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: token, logBody: true);
            var cancelPayload = await _api.LogDeserializeJson<JsonElement>(cancelResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, cancelResponse.StatusCode, "Single-round cancel status is 200 OK");
            await AssertHelper.AssertTrue(cancelPayload.Success, "Cancel request succeeds");
            await AssertHelper.AssertEqual("Booking request cancelled successfully", cancelPayload.Message, "Success message matches");
            await AssertHelper.AssertEqual((int)BookingRequestStatus.Cancelled, cancelPayload.Data!.GetProperty("status").GetInt32(), "Single-round booking status is Cancelled");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_MultipleRounds_ReturnsCancelledStatus()
        {
            var token = await LoginSeededCandidateAsync();
            var bookingId = await CreateBookingAndGetIdAsync(token, true, 23, 16);

            var cancelResponse = await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: token, logBody: true);
            var cancelPayload = await _api.LogDeserializeJson<JsonElement>(cancelResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, cancelResponse.StatusCode, "Multi-round cancel status is 200 OK");
            await AssertHelper.AssertTrue(cancelPayload.Success, "Cancel request succeeds");
            await AssertHelper.AssertEqual("Booking request cancelled successfully", cancelPayload.Message, "Success message matches");
            await AssertHelper.AssertEqual((int)BookingRequestStatus.Cancelled, cancelPayload.Data!.GetProperty("status").GetInt32(), "Multi-round booking status is Cancelled");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CancelBookingRequest_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync<object>($"/api/v1/booking-requests/{Guid.NewGuid()}/cancel", null, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_NonExistentBooking_ReturnsNotFound()
        {
            var token = await LoginSeededCandidateAsync();
            var response = await _api.PostAsync<object>($"/api/v1/booking-requests/{NonExistentBookingId}/cancel", null, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found for non-existent booking");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_AlreadyCancelled_ReturnsBadRequest()
        {
            var token = await LoginSeededCandidateAsync();
            var bookingId = await CreateBookingAndGetIdAsync(token, false, 24, 10);

            // First cancellation
            await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: token, logBody: true);

            // Second cancellation
            var response = await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for already cancelled booking");
            await AssertHelper.AssertFalse(payload.Success, "Second cancellation should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_InvalidBookingIdFormat_ReturnsBadRequest()
        {
            var token = await LoginSeededCandidateAsync();
            var response = await _api.PostAsync<object>($"/api/v1/booking-requests/invalid-guid-format/cancel", null, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid GUID format");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_ByDifferentUser_ReturnsForbidden()
        {
            var aliceToken = await LoginSeededCandidateAsync();
            var bookingId = await CreateBookingAndGetIdAsync(aliceToken, false, 25, 11);

            // Login as another user (e.g., Bob)
            var bobLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var bobToken = (await _api.LogDeserializeJson<LoginResponse>(bobLogin)).Data!.Token;

            var response = await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: bobToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden for cancelling another user's booking");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CancelBookingRequest_AsCoach_ReturnsForbidden()
        {
            // Arrange – the cancel endpoint is restricted to Candidate policy
            var loginResponse = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = COACH_EMAIL, Password = DEFAULT_PASSWORD }, logBody: true);
            var loginPayload = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var coachToken = loginPayload.Data!.Token;

            // Act
            var response = await _api.PostAsync<object>($"/api/v1/booking-requests/{Guid.NewGuid()}/cancel", null, jwtToken: coachToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Coach role cannot cancel booking requests – 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CancelBookingRequest_AsAdmin_ReturnsForbidden()
        {
            // Arrange – admin role is not in the Candidate authorization policy
            var loginResponse = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD }, logBody: true);
            var loginPayload = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var adminToken = loginPayload.Data!.Token;

            // Act
            var response = await _api.PostAsync<object>($"/api/v1/booking-requests/{Guid.NewGuid()}/cancel", null, jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Admin role cannot cancel booking requests – 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_CancelBookingRequest_NonExistentBookingId_ThrowsException()
        {
            // Arrange – a random GUID that does not exist in the database
            var token = await LoginSeededCandidateAsync();
            var nonExistentBookingId = Guid.NewGuid();

            // Act & Assert – business logic throws when the booking cannot be found
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _api.PostAsync<object>($"/api/v1/booking-requests/{nonExistentBookingId}/cancel", null, jwtToken: token, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "Exception is raised for non-existent booking ID");
        }

        private async Task<Guid> CreateBookingAndGetIdAsync(string candidateToken, bool isMultipleRounds, int dayOffset, int hourOffset)
        {
            var services = await GetCoachServicesAsync();
            var service1 = services[0];
            var service2 = services.Count > 1 ? services[1] : services[0];
            var required1 = (service1.DurationMinutes + 29) / 30;
            var required2 = isMultipleRounds ? (service2.DurationMinutes + 29) / 30 : 0;
            var allBlocks = await CreateAvailabilityBlocksAsync(required1 + required2, dayOffset, hourOffset);

            var rounds = new List<CreateInterviewRoundDto>
            {
                new() { CoachInterviewServiceId = service1.Id, AvailabilityIds = allBlocks.Take(required1).ToList() }
            };
            if (isMultipleRounds)
            {
                rounds.Add(new CreateInterviewRoundDto { CoachInterviewServiceId = service2.Id, AvailabilityIds = allBlocks.Skip(required1).Take(required2).ToList() });
            }

            var createResponse = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/jd.pdf",
                CVUrl = "https://example.com/cv.pdf",
                AimLevel = AimLevel.Senior,
                Rounds = rounds
            }, jwtToken: candidateToken, logBody: true);

            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);
            return createPayload.Data!.GetProperty("id").GetGuid();
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
            var start = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(dayOffset).Date.AddHours(hourOffset), DateTimeKind.Utc);
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
    }
}
