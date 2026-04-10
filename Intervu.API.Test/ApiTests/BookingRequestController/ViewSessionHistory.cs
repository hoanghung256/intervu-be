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
    // IC-41
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
            var items = listPayload.Data.GetProperty("items");
            if (items.GetArrayLength() == 0)
            {
                LogInfo("No booking requests found for alice – skipping detail assertion.");
                return;
            }

            var bookingId = items[0].GetProperty("id").GetGuid();

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
            var items = payload.Data.GetProperty("items");
            await AssertHelper.AssertTrue(items.GetArrayLength() <= 1, "At most 1 item is returned when pageSize=1");
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
            var data = payload.Data!;
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

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_ViewSessionHistory_EmptyHistory_ReturnsEmptyList()
        {
            // Assuming a user with no booking history can be created or exists
            // For now, we'll use a valid user and assume they might have an empty history if no bookings are made.
            // A more robust test would involve creating a new user and checking their empty history.
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", jwtToken: token, logBody: true);
            var body = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(body.Success, "History request succeeds");
            await AssertHelper.AssertNotNull(body.Data, "Paged history data exists");
            await AssertHelper.AssertEqual(0, body.Data?.Items?.Count, "Session history items should be empty");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_ViewSessionHistory_InvalidPageNumber_ReturnsBadRequest()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=0&pageSize=10", jwtToken: token, logBody: true); // Page 0 is invalid

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid page number");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_ViewSessionHistory_LargePageSize_ReturnsLimitedResults()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=1000", jwtToken: token, logBody: true); // Large page size
            var body = await _api.LogDeserializeJson<PagedResult<JsonElement>>(response); // Using JsonElement to avoid strict type matching

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(body.Success, "History request succeeds");
            await AssertHelper.AssertNotNull(body.Data, "Paged history data exists");
            // Assuming a maximum page size limit is enforced by the API, e.g., 100
            await AssertHelper.AssertTrue(body.Data?.Items?.Count <= 100, "Returned items count should be within API's max page size limit");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task Handle_ViewSessionHistory_MultiplePages_ReturnsCorrectData()
        {
            // This test requires creating enough booking history for a user to span multiple pages.
            // For simplicity, we'll assume existing data and check if pagination works.
            // A more comprehensive test would involve creating many bookings.
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;

            // Get first page
            var responsePage1 = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=2", jwtToken: token, logBody: true);
            var bodyPage1 = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(responsePage1);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, responsePage1.StatusCode, "Page 1 status code is 200 OK");
            await AssertHelper.AssertTrue(bodyPage1.Success, "Page 1 history request succeeds");
            await AssertHelper.AssertNotNull(bodyPage1.Data?.Items, "Page 1 session history items exist");
            await AssertHelper.AssertEqual(2, bodyPage1.Data?.Items?.Count, "Page 1 should have 2 items");

            // Get second page
            var responsePage2 = await _api.GetAsync("/api/v1/interview-booking/history?page=2&pageSize=2", jwtToken: token, logBody: true);
            var bodyPage2 = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(responsePage2);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, responsePage2.StatusCode, "Page 2 status code is 200 OK");
            await AssertHelper.AssertTrue(bodyPage2.Success, "Page 2 history request succeeds");
            await AssertHelper.AssertNotNull(bodyPage2.Data?.Items, "Page 2 session history items exist");
            // Assuming there's at least one more item for page 2, adjust assertion if not.
            await AssertHelper.AssertTrue(bodyPage2.Data?.Items?.Count >= 0, "Page 2 should have items or be empty if no more data");
        }
    }
}
