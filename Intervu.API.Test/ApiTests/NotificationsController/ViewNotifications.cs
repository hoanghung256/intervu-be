using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Notification;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.NotificationsController
{
    public class ViewNotificationsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewNotificationsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD }, logBody: true);
            var data = await _api.LogDeserializeJson<LoginResponse>(response, true);
            return data.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync("alice@example.com");
            var response = await _api.GetAsync("/api/v1/notifications", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<NotificationListResponseDto>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
            await AssertHelper.AssertNotNull(body.Data, "Notification data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetUnreadCount_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync("alice@example.com");
            var response = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
            await AssertHelper.AssertTrue(body.Data.TryGetInt32(out _), "Unread count is an integer");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync("alice@example.com");
            var response = await _api.PatchAsync("/api/v1/notifications/read-all", new { }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
            await AssertHelper.AssertEqual("All notifications marked as read", body.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetList_ReturnsUnauthorized_WhenUnauthenticated()
        {
            var response = await _api.GetAsync("/api/v1/notifications", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkNotificationAsRead_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync("alice@example.com");
            // First, create a notification to mark as read
            var createResponse = await _api.PostAsync("/api/v1/notifications", new CreateNotificationRequestDto
            {
                ReferenceId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"), // Alice's ID
                Title = "Test Notification",
                Message = "This is a test notification to be marked as read.",
                Type = Domain.Entities.Constants.NotificationType.SystemAnnouncement
            }, jwtToken: token, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, true);
            var notificationId = createPayload.Data.GetProperty("id").GetGuid();

            var response = await _api.PatchAsync($"/api/v1/notifications/{notificationId}/read", new { }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
            var body = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Response success is true");
            await AssertHelper.AssertEqual("Notification marked as read", body.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkNotificationAsRead_NonExistentNotification_ReturnsNotFound()
        {
            var token = await LoginAsync("alice@example.com");
            var nonExistentId = Guid.NewGuid();
            var response = await _api.PatchAsync($"/api/v1/notifications/{nonExistentId}/read", new { }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status is 404 Not Found");
        }
    }
}
