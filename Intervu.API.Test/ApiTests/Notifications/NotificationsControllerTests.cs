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
        private readonly string _aliceEmail = "alice@example.com";
        private readonly string _bobEmail = "bob@example.com";

        // Seeded notification from IntervuPostgreDbContext (belongs to Alice, IsRead=false)
        private readonly Guid _seededNotificationId = Guid.Parse("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20");

        private async Task<string> LoginAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login",
                new LoginRequest { Email = email, Password = email.Contains("admin") || email.Contains("bob") || email.Contains("alice") ? DEFAULT_PASSWORD : CANDIDATE_PASSWORD }, logBody: true);
            var data = await _api.LogDeserializeJson<LoginResponse>(response, true);
            if (!data.Success)
                throw new Exception($"Login failed for {email}");
            return data.Data!.Token;
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginUserAsync(string role = "Candidate")
        {
            var email = $"{role.ToLower()}_{Guid.NewGuid()}@example.com";
            var password = role == "Candidate" ? CANDIDATE_PASSWORD : CANDIDATE_PASSWORD;

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = $"Test {role}",
                Role = role
            });

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        // ——— GET /api/v1/notifications ——————————————————————————————————————

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("GET /api/v1/notifications as authenticated user.");
            var response = await _api.GetAsync("/api/v1/notifications", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<NotificationListResponseDto>(response, true);
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
            var body = await _api.LogDeserializeJson<NotificationListResponseDto>(response, true);

            await AssertHelper.AssertTrue(body.Data!.TotalCount >= 1, "Alice has at least 1 notification");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsEmpty_ForUserWithNoNotifications()
        {
            // Arrange - Register a new coach account who will have 0 notifications
            var (token, _) = await RegisterAndLoginUserAsync("Coach");

            LogInfo("New coach has no notifications — list should be empty.");
            var response = await _api.GetAsync("/api/v1/notifications", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<NotificationListResponseDto>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
            await AssertHelper.AssertTrue(body.Data!.TotalCount == 0, "TotalCount is 0 for new user");
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
            var body = await _api.LogDeserializeJson<NotificationListResponseDto>(response, true);
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

        // ——— GET /api/v1/notifications/unread-count ——————————————————————

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetUnreadCount_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("GET /api/v1/notifications/unread-count");
            var response = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<object>(response, true);
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
            // Arrange - Register a new coach
            var (token, _) = await RegisterAndLoginUserAsync("Coach");

            LogInfo("New coach's unread count must be 0.");
            var response = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");

            var content = await response.Content.ReadAsStringAsync();
            var count = JsonDocument.Parse(content).RootElement.GetProperty("data").GetProperty("count").GetInt32();
            await AssertHelper.AssertTrue(count == 0, "Unread count is 0 for new user");
        }

        // ——— PATCH /api/v1/notifications/{id}/read ———————————————————————

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAsRead_ReturnsOk_WhenNotificationExists()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo($"PATCH /api/v1/notifications/{_seededNotificationId}/read");
            var response = await _api.PatchAsync(
                $"/api/v1/notifications/{_seededNotificationId}/read", new { },
                jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Notification marked as read successfully");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAsRead_ReturnsUnauthorized_WhenNoToken()
        {
            LogInfo("PATCH /{id}/read without token.");
            var response = await _api.PatchAsync($"/api/v1/notifications/{_seededNotificationId}/read", new { });

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAsRead_WithNonExistentId_ReturnsOk()
        {
            var token = await LoginAsync(_aliceEmail);
            var randomGuid = Guid.NewGuid();

            LogInfo($"PATCH with non-existent notification ID {randomGuid}.");
            var response = await _api.PatchAsync($"/api/v1/notifications/{randomGuid}/read", new { }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK (silent ignore for missing ID)");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAsRead_WithInvalidGuidFormat_ReturnsBadRequest()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("PATCH with invalid GUID in route.");
            var response = await _api.PatchAsync("/api/v1/notifications/not-a-guid/read", new { }, jwtToken: token);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status is 400 Bad Request for invalid GUID");
        }

        // ——— PATCH /api/v1/notifications/read-all —————————————————————————

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("PATCH /api/v1/notifications/read-all");
            var response = await _api.PatchAsync("/api/v1/notifications/read-all", new { }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(body.Success, "All notifications marked as read");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ReturnsUnauthorized_WhenNoToken()
        {
            LogInfo("PATCH /read-all without token.");
            var response = await _api.PatchAsync("/api/v1/notifications/read-all", new { });

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ThenUnreadCountIsZero()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("Marking all notifications as read.");
            await _api.PatchAsync("/api/v1/notifications/read-all", new { }, jwtToken: token);

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
            // Arrange - Use a fresh coach who definitely has 0 notifications
            var (token, _) = await RegisterAndLoginUserAsync("Coach");

            LogInfo("PATCH /read-all for new user with no notifications.");
            var response = await _api.PatchAsync("/api/v1/notifications/read-all", new { }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK even with no notifications to mark");
        }

        // ——— Cross-user isolation ———————————————————————————————————————————

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_DoesNotReturnOtherUsersNotifications()
        {
            // Arrange - Use Bob
            var bobToken = await LoginAsync(_bobEmail);
 
            LogInfo("Verifying Bob cannot see Alice's seeded notification.");
            var response = await _api.GetAsync("/api/v1/notifications", jwtToken: bobToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;
            var items = json.GetProperty("data").GetProperty("items");

            var containsAliceNotification = false;
            foreach (var item in items.EnumerateArray())
            {
                if (item.GetProperty("id").GetString() == _seededNotificationId.ToString())
                {
                    containsAliceNotification = true;
                    break;
                }
            }
            await AssertHelper.AssertTrue(!containsAliceNotification, "Bob does not see Alice's seeded notification");
        }

        // --- POST /api/v1/notifications/admin (Admin: send to specific user) ---

        private readonly string _adminEmail = "admin@example.com";
        private readonly Guid _aliceUserId = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
        private readonly Guid _bobUserId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminCreate_ReturnsForbidden_WhenNotAdmin()
        {
            var token = await LoginAsync(_aliceEmail);

            LogInfo("POST /admin as non-admin user should be 403.");
            var response = await _api.PostAsync("/api/v1/notifications/admin",
                new CreateNotificationRequestDto
                {
                    UserId = _bobUserId,
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "Test",
                    Message = "Should fail"
                }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-admin gets 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminCreate_ReturnsUnauthorized_WhenNoToken()
        {
            LogInfo("POST /admin without token should be 401.");
            var response = await _api.PostAsync("/api/v1/notifications/admin",
                new CreateNotificationRequestDto
                {
                    UserId = _bobUserId,
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "Test",
                    Message = "Should fail"
                }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token gets 401");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminCreate_ReturnsOk_WhenAdmin()
        {
            var token = await LoginAsync(_adminEmail);

            LogInfo("POST /admin as admin to send notification to Bob.");
            var response = await _api.PostAsync("/api/v1/notifications/admin",
                new CreateNotificationRequestDto
                {
                    UserId = _bobUserId,
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "Admin Test",
                    Message = "This is a test notification from admin"
                }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Admin gets 200 OK");
            var body = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminCreate_ThenUserSeesNotification()
        {
            var adminToken = await LoginAsync(_adminEmail);
            var bobToken = await LoginAsync(_bobEmail);

            var beforeResponse = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: bobToken);
            var beforeContent = await beforeResponse.Content.ReadAsStringAsync();
            var beforeCount = JsonDocument.Parse(beforeContent).RootElement.GetProperty("data").GetProperty("count").GetInt32();

            LogInfo("Admin sends notification to Bob.");
            await _api.PostAsync("/api/v1/notifications/admin",
                new CreateNotificationRequestDto
                {
                    UserId = _bobUserId,
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "End-to-end test",
                    Message = "Bob should see this"
                }, jwtToken: adminToken, logBody: true);

            LogInfo("Verifying Bob's unread count increased.");
            var afterResponse = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: bobToken);
            var afterContent = await afterResponse.Content.ReadAsStringAsync();
            var afterCount = JsonDocument.Parse(afterContent).RootElement.GetProperty("data").GetProperty("count").GetInt32();

            await AssertHelper.AssertTrue(afterCount > beforeCount, $"Bob's unread count increased from {beforeCount} to {afterCount}");
        }

        // --- POST /api/v1/notifications/admin/broadcast ---

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminBroadcast_ReturnsOk_WhenAdmin()
        {
            var token = await LoginAsync(_adminEmail);

            LogInfo("POST /admin/broadcast to Alice and Bob.");
            var response = await _api.PostAsync("/api/v1/notifications/admin/broadcast",
                new BroadcastNotificationRequestDto
                {
                    UserIds = new List<Guid> { _aliceUserId, _bobUserId },
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "Broadcast Test",
                    Message = "Sent to multiple users"
                }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Admin broadcast returns 200 OK");
            var body = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminBroadcast_ReturnsBadRequest_WhenUserIdsEmpty()
        {
            var token = await LoginAsync(_adminEmail);

            LogInfo("POST /admin/broadcast with empty UserIds.");
            var response = await _api.PostAsync("/api/v1/notifications/admin/broadcast",
                new BroadcastNotificationRequestDto
                {
                    UserIds = new List<Guid>(),
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "Empty",
                    Message = "Should fail"
                }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Empty UserIds returns 400");
        }

        // --- POST /api/v1/notifications/admin/broadcast-all ---

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminBroadcastAll_ReturnsOk_WhenAdmin()
        {
            var token = await LoginAsync(_adminEmail);

            LogInfo("POST /admin/broadcast-all to all users.");
            var response = await _api.PostAsync("/api/v1/notifications/admin/broadcast-all",
                new BroadcastAllRequestDto
                {
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "System Maintenance",
                    Message = "System will be down for maintenance tonight"
                }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Broadcast-all returns 200 OK");
            var body = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
        }

        // --- POST /api/v1/notifications/admin/broadcast-role ---

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminBroadcastRole_ReturnsOk_WhenAdmin()
        {
            var token = await LoginAsync(_adminEmail);

            LogInfo("POST /admin/broadcast-role to Coach role.");
            var response = await _api.PostAsync("/api/v1/notifications/admin/broadcast-role",
                new BroadcastRoleRequestDto
                {
                    Role = "Coach",
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "Coach Policy Update",
                    Message = "Commission rate changed"
                }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Broadcast-role returns 200 OK");
            var body = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminBroadcastRole_ReturnsBadRequest_WhenRoleEmpty()
        {
            var token = await LoginAsync(_adminEmail);

            LogInfo("POST /admin/broadcast-role with empty Role.");
            var response = await _api.PostAsync("/api/v1/notifications/admin/broadcast-role",
                new BroadcastRoleRequestDto
                {
                    Role = "",
                    Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                    Title = "Empty",
                    Message = "Should fail"
                }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Empty Role returns 400");
        }
    }
}