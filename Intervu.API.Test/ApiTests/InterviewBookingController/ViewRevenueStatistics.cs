using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class ViewRevenueStatisticsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        // IC-56
        private readonly ApiHelper _api;

        public ViewRevenueStatisticsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<string> LoginUserAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetRevenueStatistics_Success_ReturnsOk()
        {
            var token = await LoginAdminAsync();

            var response = await _api.GetAsync("/api/v1/admin/revenue-statistics?startDate=2023-01-01&endDate=2023-12-31", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request successful");
            await AssertHelper.AssertNotNull(payload.Data, "Revenue data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetRevenueStatistics_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync("/api/v1/admin/revenue-statistics?startDate=2023-01-01&endDate=2023-12-31", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetRevenueStatistics_Forbidden_ReturnsForbidden()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.GetAsync("/api/v1/admin/revenue-statistics?startDate=2023-01-01&endDate=2023-12-31", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-admin user should get 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetRevenueStatistics_InvalidDates_ReturnsBadRequest()
        {
            var token = await LoginAdminAsync();

            // End date before start date
            var response = await _api.GetAsync("/api/v1/admin/revenue-statistics?startDate=2023-12-31&endDate=2023-01-01", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid date range should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetRevenueStatistics_MissingDates_ReturnsBadRequest()
        {
            var token = await LoginAdminAsync();

            var response = await _api.GetAsync("/api/v1/admin/revenue-statistics", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing start/end dates should return 400 Bad Request");
        }
    }
}
