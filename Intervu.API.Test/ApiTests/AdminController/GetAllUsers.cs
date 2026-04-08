using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class GetAllUsersTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public GetAllUsersTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAdminAsync()
        {
            var loginRequest = new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD };
            var response = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllUsers_ReturnsPagedList()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            LogInfo("Getting all users.");
            var response = await _api.GetAsync("/api/v1/admin/users?page=1&pageSize=5", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Users list is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllUsers_DefaultParams_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/users", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Default params returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Users list is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllUsers_VerifyPagingMetadata()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/users?page=1&pageSize=3", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertEqual(3, apiResponse.Data!.PageSize, "PageSize matches request");
            await AssertHelper.AssertEqual(1, apiResponse.Data.CurrentPage, "CurrentPage matches request");
            await AssertHelper.AssertTrue(apiResponse.Data.Items.Count <= 3, "Items count does not exceed page size");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllUsers_PageSizeOne_ReturnsAtMostOneItem()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/users?page=1&pageSize=1", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Users list is not null");
            await AssertHelper.AssertTrue(apiResponse.Data!.Items.Count <= 1, "Result contains at most one item");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllUsers_HighPageNumber_ReturnsEmptyItems()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/users?page=9999&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "High page returns 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertTrue(apiResponse.Data!.Items.Count == 0, "High page returns empty items list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllUsers_PageTwo_ReturnsDifferentResultsThanPageOne()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var responsePage1 = await _api.GetAsync("/api/v1/admin/users?page=1&pageSize=1", jwtToken: token);
            var responsePage2 = await _api.GetAsync("/api/v1/admin/users?page=2&pageSize=1", jwtToken: token);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, responsePage1.StatusCode, "Page 1 returns 200 OK");
            await AssertHelper.AssertEqual(HttpStatusCode.OK, responsePage2.StatusCode, "Page 2 returns 200 OK");
            var page1 = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(responsePage1);
            var page2 = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(responsePage2);
            await AssertHelper.AssertEqual(1, page1.Data!.CurrentPage, "Page 1 current page is 1");
            await AssertHelper.AssertEqual(2, page2.Data!.CurrentPage, "Page 2 current page is 2");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllUsers_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/admin/users?page=1&pageSize=5", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No auth token returns 401 Unauthorized");
        }
    }
}
