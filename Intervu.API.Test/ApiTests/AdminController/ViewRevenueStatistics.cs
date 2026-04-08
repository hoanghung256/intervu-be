using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class ViewRevenueStatisticsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
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

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetDashboardStats_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/stats", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<DashboardStatsDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Dashboard stats data is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetDashboardStats_ReturnsTotalUsersGreaterThanZero()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/stats", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<DashboardStatsDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Data!.TotalUsers > 0, "TotalUsers is greater than zero");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetDashboardStats_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/admin/stats", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No auth token returns 401 Unauthorized");
        }
    }
}
