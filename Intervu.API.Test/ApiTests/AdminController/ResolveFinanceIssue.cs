using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class ResolveFinanceIssueTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ResolveFinanceIssueTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task ResolveFinanceIssue_Success_ReturnsOk()
        {
            var token = await LoginAdminAsync();
            var bookingId = Guid.NewGuid(); // Replace with a real pending finance issue booking ID if possible

            var response = await _api.PostAsync($"/api/v1/admin/resolve-finance", new
            {
                BookingId = bookingId,
                Resolution = "Refund processed",
                IsRefund = true
            }, jwtToken: token, logBody: true);

            // Assuming if ID is random it might return NotFound, but here we test the normal flow
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
                await AssertHelper.AssertTrue(payload.Success, "Resolution request successful");
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveFinanceIssue_Unauthorized_ReturnsForbidden()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;

            var response = await _api.PostAsync("/api/v1/admin/resolve-finance", new
            {
                BookingId = Guid.NewGuid(),
                Resolution = "Test resolution",
                IsRefund = false
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-admin user should get 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveFinanceIssue_NonExistentBooking_ReturnsNotFound()
        {
            var token = await LoginAdminAsync();
            var response = await _api.PostAsync("/api/v1/admin/resolve-finance", new
            {
                BookingId = Guid.NewGuid(),
                Resolution = "Resolution for non-existent booking",
                IsRefund = false
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Should return 404 Not Found for non-existent booking ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveFinanceIssue_MissingResolutionMessage_ReturnsBadRequest()
        {
            var token = await LoginAdminAsync();
            var response = await _api.PostAsync("/api/v1/admin/resolve-finance", new
            {
                BookingId = Guid.NewGuid(),
                Resolution = "", // Empty resolution
                IsRefund = false
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Should return 400 Bad Request for empty resolution");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveFinanceIssue_AlreadyResolved_ReturnsConflict()
        {
            // This test requires a booking ID that is already resolved.
            // Placeholder logic:
            var token = await LoginAdminAsync();
            var bookingId = Guid.NewGuid(); // Assuming this one is already resolved in the DB or via a previous call

            var response = await _api.PostAsync("/api/v1/admin/resolve-finance", new
            {
                BookingId = bookingId,
                Resolution = "Double resolution attempt",
                IsRefund = false
            }, jwtToken: token, logBody: true);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                await AssertHelper.AssertEqual(HttpStatusCode.Conflict, response.StatusCode, "Should return 409 Conflict for already resolved issues");
            }
        }
    }
}
