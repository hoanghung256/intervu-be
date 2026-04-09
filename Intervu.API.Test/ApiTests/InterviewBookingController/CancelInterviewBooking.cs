using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    // IC-33
    public class CancelInterviewBookingTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        // room1Id: Status=Scheduled, ScheduledTime=2026-02-10 (in the past as of test run date 2026-04-09).
        // IsAvailableForCancel() returns false → BadRequestException → middleware maps to 400.
        private readonly Guid _pastScheduledRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");

        public CancelInterviewBookingTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAsAliceAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        // ── Abnormal: Auth failures ─────────────────────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CancelInterview_ReturnsUnauthorized_WhenNoToken()
        {
            // Act – no Authorization header
            var response = await _api.PostAsync<object>($"/api/v1/interview-booking/cancel/{_pastScheduledRoomId}", null, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized without token");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CancelInterview_ReturnsForbidden_WhenCalledByCoach()
        {
            // Arrange – bob is a Coach; the cancel endpoint requires Candidate policy
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var coachToken = loginData.Data!.Token;

            // Act
            var response = await _api.PostAsync<object>($"/api/v1/interview-booking/cancel/{_pastScheduledRoomId}", null, jwtToken: coachToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden for Coach role");
        }

        // ── Boundary: Non-existent room ─────────────────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CancelInterview_ReturnsNotFound_WhenRoomDoesNotExist()
        {
            // Arrange – random Guid has no matching InterviewRoom row
            var aliceToken = await LoginAsAliceAsync();
            var nonExistentRoomId = Guid.NewGuid();

            // Act
            var response = await _api.PostAsync<object>($"/api/v1/interview-booking/cancel/{nonExistentRoomId}", null, jwtToken: aliceToken, logBody: true);

            // Assert – CancelInterview throws NotFoundException → ExceptionHandlingMiddleware maps to 404
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found for non-existent room");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertFalse(payload.Success, "Response success flag is false");
        }

        // ── Abnormal: Room not in cancellable state ──────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task CancelInterview_ReturnsBadRequest_WhenRoomIsNotCancellable()
        {
            // Arrange – room1 is Scheduled but its ScheduledTime (2026-02-10) is in the past.
            // IsAvailableForCancel() returns false → BadRequestException → middleware maps to 400.
            var aliceToken = await LoginAsAliceAsync();

            // Act
            var response = await _api.PostAsync<object>($"/api/v1/interview-booking/cancel/{_pastScheduledRoomId}", null, jwtToken: aliceToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for a room whose scheduled time has passed");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertFalse(payload.Success, "Response success flag is false");
        }
    }
}
