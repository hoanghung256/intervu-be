using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    // IC-55
    public class ViewAllPaymentHistoryTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewAllPaymentHistoryTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllPayments_ReturnsSuccess_WhenAdminAuthenticated()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/payments?page=1&pageSize=10", jwtToken: token, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response, true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Payments list is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllPayments_VerifyPagingMetadata()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/payments?page=1&pageSize=5", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertEqual(5, apiResponse.Data!.PageSize, "PageSize matches request");
            await AssertHelper.AssertEqual(1, apiResponse.Data.CurrentPage, "CurrentPage matches request");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllPayments_ReturnsSuccess_WithSmallPageSize()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/payments?page=1&pageSize=1", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertEqual(1, apiResponse.Data!.PageSize, "Page size follows request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllPayments_HighPageNumber_ReturnsEmptyItems()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/payments?page=9999&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "High page returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertTrue(apiResponse.Data!.Items.Count == 0, "High page returns empty items");
        }

        // ===== [A] Abnormal / Error Path Tests =====
        // NOTE: The payments endpoint currently has NO authorization — these tests document the current behavior.

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllPayments_ReturnsSuccess_WhenNoToken_CurrentBehavior()
        {
            // Act - No auth token, endpoint is NOT secured
            var response = await _api.GetAsync("/api/v1/admin/payments?page=1&pageSize=10", logBody: true);

            // Assert - Documents that this endpoint lacks authorization
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "No token currently returns 200 OK (endpoint not secured)");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllPayments_ReturnsSuccess_WhenUserIsNotAdmin_CurrentBehavior()
        {
            // Arrange
            var token = await LoginUserAsync("alice@example.com");

            // Act
            var response = await _api.GetAsync("/api/v1/admin/payments?page=1&pageSize=10", jwtToken: token, logBody: true);

            // Assert - Documents that this endpoint lacks role authorization
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Non-admin currently gets 200 OK (endpoint not secured)");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }
    }
}
