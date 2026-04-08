using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class ViewUserAccountsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public ViewUserAccountsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var data = await _api.LogDeserializeJson<LoginResponse>(response);
            return data.Data!.Token;
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task Handle_AdminRequest_ReturnsPagedUsers()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/users?page=1&pageSize=5", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var body = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(body.Data?.Items, "Users list is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAllUsers_DefaultPaging_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/users", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Default paging returns 200 OK");
            var body = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(body.Data?.Items, "Users list is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetUserById_ExistingUser_ReturnsUserDetails()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var usersResponse = await _api.GetAsync("/api/v1/admin/users?page=1&pageSize=1", jwtToken: token);
            var users = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(usersResponse);
            var firstUser = users.Data!.Items.First();

            // Act
            var response = await _api.GetAsync($"/api/v1/admin/users/{firstUser.Id}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Get user by ID returns 200 OK");
            var result = await _api.LogDeserializeJson<Intervu.Application.DTOs.Admin.UserDto>(response);
            await AssertHelper.AssertTrue(result.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(result.Data, "User data is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task FilterUsers_ByRoleCandidate_ReturnsFilteredResults()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/users/filter?page=1&pageSize=10&role=Candidate", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Filter by Candidate role returns 200 OK");
            var body = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(body.Data?.Items, "Filtered users list is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task FilterUsers_BySearchTerm_ReturnsFilteredResults()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/users/filter?page=1&pageSize=10&search=admin", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Filter by search returns 200 OK");
            var body = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "API response indicates success");
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
            var body = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "API response indicates success");
            await AssertHelper.AssertTrue(body.Data!.Items.Count <= 1, "Result contains at most one item");
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
            var body = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "API response indicates success");
            await AssertHelper.AssertTrue(body.Data!.Items.Count == 0, "High page returns empty items");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetUserById_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _api.GetAsync($"/api/v1/admin/users/{nonExistentId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent user ID returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetUserById_EmptyGuid_ReturnsNotFound()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync($"/api/v1/admin/users/{Guid.Empty}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Empty GUID returns 404 NotFound");
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
