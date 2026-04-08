using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Notification;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.NotificationsController
{
    public class CreateNotificationTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _bobUserId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        public CreateNotificationTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public async Task AdminCreate_ReturnsOk_WhenAdmin()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "admin@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PostAsync("/api/v1/notifications/admin", new CreateNotificationRequestDto
            {
                UserId = _bobUserId,
                Type = NotificationType.SystemAnnouncement,
                Title = "Admin Test",
                Message = "This is a test notification from admin"
            }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Admin gets 200 OK");
        }

        [Fact(Skip = "Broadcast variations are covered in legacy tests and can be split further if required.")]
        [Trait("Category", "API")]
        [Trait("Category", "Notifications")]
        public Task BroadcastNotification_Placeholder() => Task.CompletedTask;
    }
}
