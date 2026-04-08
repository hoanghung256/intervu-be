using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using System.Net;
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

        private async Task<string> LoginUserAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        // ===== [N] Normal / Happy Path Tests =====
        // Tests for the companies and feedbacks endpoints — admin-accessible finance/review data

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllCompanies_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/companies?page=1&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Companies list is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllFeedbacks_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/feedbacks?page=1&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Feedbacks list is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllInterviewers_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/interviewers?page=1&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Interviewers list is returned");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllCompanies_HighPageNumber_ReturnsEmptyItems()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/companies?page=9999&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "High page returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertTrue(apiResponse.Data!.Items.Count == 0, "High page returns empty items");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllFeedbacks_PageSizeOne_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/feedbacks?page=1&pageSize=1", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Small page size returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<object>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllCompanies_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/admin/companies?page=1&pageSize=10", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No auth token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllFeedbacks_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/admin/feedbacks?page=1&pageSize=10", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No auth token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllInterviewers_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/admin/interviewers?page=1&pageSize=10", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No auth token returns 401 Unauthorized");
        }
    }
}
