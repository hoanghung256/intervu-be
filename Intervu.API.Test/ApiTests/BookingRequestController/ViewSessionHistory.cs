using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewBooking;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.BookingRequestController
{
    public class ViewSessionHistoryTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public ViewSessionHistoryTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        // ── /interview-booking/history ──────────────────────────────────────────

        [Fact]
        public async Task Handle_AuthenticatedUser_ReturnsSessionHistory()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var body = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "History request succeeds");
            await AssertHelper.AssertNotNull(body.Data, "Paged history data exists");
            await AssertHelper.AssertNotNull(body.Data?.Items, "Session history items exist");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_ViewSessionHistory_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        // ── GET /booking-requests  (list endpoint) ──────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetMyBookingRequests_AsCandidate_ReturnsSuccess()
        {
            // Arrange
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;

            // Act
            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Get booking requests succeeds");
            await AssertHelper.AssertNotNull(payload.Data, "Booking requests data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetMyBookingRequests_AsCoach_ReturnsSuccess()
        {
            // Arrange – coaches can also list their own incoming booking requests
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = COACH_EMAIL, Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;

            // Act
            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Coach can list booking requests – 200 OK");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Coach booking request list succeeds");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetMyBookingRequests_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=10", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token returns 401 Unauthorized");
        }

        // ── GET /booking-requests/{id}  (detail endpoint) ──────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetBookingRequestDetail_WithValidId_ReturnsSuccess()
        {
            // Arrange – reuse alice's first booking request from the list
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var listResponse = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=1", jwtToken: token);
            var listPayload = await _api.LogDeserializeJson<JsonElement>(listResponse);

            // Only run the detail assertion if alice has at least one booking
            var items = listPayload.Data?.GetProperty("items");
            if (items == null || items.Value.GetArrayLength() == 0)
            {
                LogInfo("No booking requests found for alice – skipping detail assertion.");
                return;
            }

            var bookingId = items.Value[0].GetProperty("id").GetGuid();

            // Act
            var response = await _api.GetAsync($"/api/v1/booking-requests/{bookingId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Detail request returns 200 OK");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Booking request detail succeeds");
            await AssertHelper.AssertNotNull(payload.Data, "Detail data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetBookingRequestDetail_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync($"/api/v1/booking-requests/{Guid.NewGuid()}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token returns 401 Unauthorized");
        }

        // ── Additional edge-case tests ──────────────────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetMyBookingRequests_AsAdmin_ReturnsForbidden()
        {
            // Arrange – admin role is not in the CandidateOrInterviewer policy
            var login = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminToken = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;

            // Act
            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=10", jwtToken: adminToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Admin role returns 403 Forbidden on booking request list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetMyBookingRequests_WithPageSizeOne_ReturnsAtMostOneItem()
        {
            // Arrange
            var login = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;

            // Act
            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=1", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "PageSize=1 returns 200 OK");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds");
            var items = payload.Data?.GetProperty("items");
            await AssertHelper.AssertTrue(items?.GetArrayLength() <= 1, "At most 1 item is returned when pageSize=1");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetMyBookingRequests_ResponseContainsPaginationFields()
        {
            // Arrange
            var login = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;

            // Act
            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=5", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds");
            var data = payload.Data!.Value;
            await AssertHelper.AssertTrue(data.TryGetProperty("items", out _), "Response data contains items field");
            await AssertHelper.AssertTrue(data.TryGetProperty("totalCount", out _), "Response data contains totalCount field");
            await AssertHelper.AssertTrue(data.TryGetProperty("page", out _), "Response data contains page field");
            await AssertHelper.AssertTrue(data.TryGetProperty("pageSize", out _), "Response data contains pageSize field");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_GetBookingRequestDetail_NonExistentId_ThrowsException()
        {
            // Arrange
            var login = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _api.GetAsync($"/api/v1/booking-requests/{nonExistentId}", jwtToken: token, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "Exception is raised for non-existent booking request ID");
        }
    }
}
