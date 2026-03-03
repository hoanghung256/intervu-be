using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Notification;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Notifications
{
    public class NotificationsControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public NotificationsControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        // Seeded users from IntervuPostgreDbContext
        private readonly string _aliceEmail = "alice@example.com"; // Candidate — has 1 seeded notification
        private readonly string _bobEmail = "bob@example.com";   // Coach — has 0 notifications

        // Seeded notification from IntervuPostgreDbContext (belongs to Alice, IsRead=false)
        private readonly Guid _seededNotificationId = Guid.Parse("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20");

        private async Task<string> LoginAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = email, Password = ACCOUNT_PASSWORD });
            var data = await _api.LogDeserializeJson<LoginResponse>(response);
            if (!data.Success)
                throw new Exception($"Login failed for {email}");
            return data.Data!.Token;
        }

        // ─── GET /api/v1/notifications ───────────────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("GET /api/v1/notifications as authenticated user.");
            var response = await _api.GetAsync("/api/v1/notifications", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<NotificationListResponse>(response);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
            await AssertHelper.AssertNotNull(body.Data, "Data is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsUnauthorized_WhenNoToken()
        {
            LogInfo("GET /api/v1/notifications without token.");
            var response = await _api.GetAsync("/api/v1/notifications");

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ResponseContainsRequiredFields()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("Checking that response body has items, totalCount, unreadCount, page, pageSize.");
            var response = await _api.GetAsync("/api/v1/notifications?page=1&pageSize=20", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;
            var data = json.GetProperty("data");

            await AssertHelper.AssertTrue(data.TryGetProperty("items", out _), "Response has 'items' field");
            await AssertHelper.AssertTrue(data.TryGetProperty("totalCount", out _), "Response has 'totalCount' field");
            await AssertHelper.AssertTrue(data.TryGetProperty("unreadCount", out _), "Response has 'unreadCount' field");
            await AssertHelper.AssertTrue(data.TryGetProperty("page", out _), "Response has 'page' field");
            await AssertHelper.AssertTrue(data.TryGetProperty("pageSize", out _), "Response has 'pageSize' field");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsSeededNotification_ForAlice()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("Alice should have at least 1 seeded notification.");
            var response = await _api.GetAsync("/api/v1/notifications", jwtToken: token, logBody: true);
            var body = await _api.LogDeserializeJson<NotificationListResponse>(response);

            await AssertHelper.AssertTrue(body.Data!.TotalCount >= 1, "Alice has at least 1 notification");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsEmpty_ForUserWithNoNotifications()
        {
            var token = await LoginAsync(_bobEmail);

            LogInfo("Bob (coach) has no seeded notifications — list should be empty.");
            var response = await _api.GetAsync("/api/v1/notifications", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<NotificationListResponse>(response);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
            await AssertHelper.AssertTrue(body.Data!.TotalCount == 0, "TotalCount is 0 for user with no notifications");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsOk_WithPageSize()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("GET /api/v1/notifications?page=1&pageSize=5");
            var response = await _api.GetAsync("/api/v1/notifications?page=1&pageSize=5", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<NotificationListResponse>(response);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsEmptyItems_WhenPageExceedsTotalCount()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("Requesting page 999 — should return empty items but still 200 OK.");
            var response = await _api.GetAsync("/api/v1/notifications?page=999&pageSize=20", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;
            var items = json.GetProperty("data").GetProperty("items");
            await AssertHelper.AssertTrue(items.GetArrayLength() == 0, "Items is empty for out-of-range page");
        }

        // ─── GET /api/v1/notifications/unread-count ──────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetUnreadCount_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("GET /api/v1/notifications/unread-count");
            var response = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
            await AssertHelper.AssertNotNull(body.Data, "Count data is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetUnreadCount_ReturnsUnauthorized_WhenNoToken()
        {
            LogInfo("GET /notifications/unread-count without token.");
            var response = await _api.GetAsync("/api/v1/notifications/unread-count");

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetUnreadCount_ReturnsZero_ForUserWithNoNotifications()
        {
            var token = await LoginAsync(_bobEmail);

            LogInfo("Bob has no notifications so unread count must be 0.");
            var response = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");

            var content = await response.Content.ReadAsStringAsync();
            var count = JsonDocument.Parse(content).RootElement.GetProperty("data").GetProperty("count").GetInt32();
            await AssertHelper.AssertTrue(count == 0, "Unread count is 0 for Bob who has no notifications");
        }

        // ─── PATCH /api/v1/notifications/{id}/read ───────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAsRead_ReturnsOk_WhenNotificationExists()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo($"PATCH /api/v1/notifications/{_seededNotificationId}/read");
            var response = await _api.PatchAsync(
                $"/api/v1/notifications/{_seededNotificationId}/read",
                jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(body.Success, "Notification marked as read successfully");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAsRead_ReturnsUnauthorized_WhenNoToken()
        {
            LogInfo("PATCH /{id}/read without token.");
            var response = await _api.PatchAsync($"/api/v1/notifications/{_seededNotificationId}/read");

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAsRead_WithNonExistentId_ReturnsOk()
        {
            // The controller calls MarkAsReadAsync which silently returns if ID not found (no exception).
            // This documents the "silent ignore" behavior for non-existent IDs.
            var token = await LoginAsync(_aliceEmail);
            var randomGuid = Guid.NewGuid();

            LogInfo($"PATCH with non-existent notification ID {randomGuid}.");
            var response = await _api.PatchAsync($"/api/v1/notifications/{randomGuid}/read", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK (silent ignore for missing ID)");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAsRead_WithInvalidGuidFormat_ReturnsBadRequest()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("PATCH with invalid GUID in route.");
            var response = await _api.PatchAsync("/api/v1/notifications/not-a-guid/read", jwtToken: token);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status is 400 Bad Request for invalid GUID");
        }

        // ─── PATCH /api/v1/notifications/read-all ────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("PATCH /api/v1/notifications/read-all");
            var response = await _api.PatchAsync("/api/v1/notifications/read-all", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(body.Success, "All notifications marked as read");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ReturnsUnauthorized_WhenNoToken()
        {
            LogInfo("PATCH /read-all without token.");
            var response = await _api.PatchAsync("/api/v1/notifications/read-all");

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ThenUnreadCountIsZero()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("Marking all notifications as read.");
            await _api.PatchAsync("/api/v1/notifications/read-all", jwtToken: token);

            LogInfo("Verifying unread count drops to 0.");
            var countResponse = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, countResponse.StatusCode, "Status is 200 OK");

            var content = await countResponse.Content.ReadAsStringAsync();
            var count = JsonDocument.Parse(content).RootElement.GetProperty("data").GetProperty("count").GetInt32();
            await AssertHelper.AssertTrue(count == 0, "Unread count is 0 after mark-all-as-read");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ReturnsOk_WhenUserHasNoNotifications()
        {
            // Edge case: calling mark-all-as-read when there are no unread notifications should still be 200.
            var token = await LoginAsync(_bobEmail);

            LogInfo("PATCH /read-all for Bob who has no notifications.");
            var response = await _api.PatchAsync("/api/v1/notifications/read-all", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK even with no notifications to mark");
        }

        // ─── Cross-user isolation ─────────────────────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_DoesNotReturnOtherUsersNotifications()
        {
            // Bob (coach) should never see Alice's notifications
            var bobToken = await LoginAsync(_bobEmail);

            LogInfo("Verifying Bob cannot see Alice's notifications.");
            var response = await _api.GetAsync("/api/v1/notifications", jwtToken: bobToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<NotificationListResponse>(response);

            await AssertHelper.AssertTrue(body.Data!.TotalCount == 0, "Bob sees 0 notifications (not Alice's)");
        }
    }
}
