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
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task GetUnreadCount_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync("alice@example.com");
            var response = await _api.GetAsync("/api/v1/notifications/unread-count", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task MarkAllAsRead_ReturnsOk_WhenAuthenticated()
        {
            var token = await LoginAsync("alice@example.com");
            var response = await _api.PatchAsync("/api/v1/notifications/read-all", new { }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status is 200 OK");
        }
    }
}
