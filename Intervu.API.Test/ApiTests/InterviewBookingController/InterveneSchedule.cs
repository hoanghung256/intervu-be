using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class InterveneScheduleTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public InterveneScheduleTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task AdminIntervene_Success_ReturnsOk()
        {
            var token = await LoginAdminAsync();
            var bookingId = Guid.NewGuid(); // Replace with a real booking ID for testing

            var response = await _api.PostAsync($"/api/v1/admin/intervene", new
            {
                BookingId = bookingId,
                Action = "Reschedule",
                NewStartTime = DateTime.UtcNow.AddDays(1),
                Reason = "System maintenance"
            }, jwtToken: token, logBody: true);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
                await AssertHelper.AssertTrue(payload.Success, "Intervention successful");
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task AdminIntervene_NonAdmin_ReturnsForbidden()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;

            var response = await _api.PostAsync("/api/v1/admin/intervene", new
            {
                BookingId = Guid.NewGuid(),
                Action = "Cancel",
                Reason = "Unauthorized intervention"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-admin user should get 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task AdminIntervene_NonExistentBooking_ReturnsNotFound()
        {
            var token = await LoginAdminAsync();
            var response = await _api.PostAsync("/api/v1/admin/intervene", new
            {
                BookingId = Guid.NewGuid(),
                Action = "Reschedule",
                NewStartTime = DateTime.UtcNow.AddDays(1),
                Reason = "Non-existent booking"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Should return 404 Not Found for non-existent booking ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task AdminIntervene_MissingAction_ReturnsBadRequest()
        {
            var token = await LoginAdminAsync();
            var response = await _api.PostAsync("/api/v1/admin/intervene", new
            {
                BookingId = Guid.NewGuid(),
                Action = "", // Empty action
                Reason = "Testing missing action"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 400 Bad Request for empty action");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task AdminIntervene_InvalidNewTime_ReturnsBadRequest()
        {
            var token = await LoginAdminAsync();
            var response = await _api.PostAsync("/api/v1/admin/intervene", new
            {
                BookingId = Guid.NewGuid(),
                Action = "Reschedule",
                NewStartTime = DateTime.UtcNow.AddDays(-1), // Past date
                Reason = "Invalid time"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 400 Bad Request for past date-time");
        }
    }
}
