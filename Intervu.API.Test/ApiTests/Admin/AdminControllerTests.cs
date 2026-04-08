using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;
using Intervu.Domain.Entities.Constants;
using Intervu.Application.DTOs.Common;

namespace Intervu.API.Test.ApiTests.Admin
{
    public class AdminControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public AdminControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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

//         [Fact]
//         [Trait("Category", "API")]
//         [Trait("Category", "Admin")]
//         public async Task GetDashboardStats_ReturnsSuccess()
//         {
//             // Arrange
//             var token = await LoginAdminAsync();
//
//             // Act
//             LogInfo("Getting dashboard stats.");
//             var response = await _api.GetAsync("/api/v1/admin/stats", jwtToken: token, logBody: true);
//
//             // Assert
//             await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
//             var apiResponse = await _api.LogDeserializeJson<object>(response);
//             await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
//         }

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
            var apiResponse = await _api.LogDeserializeJson<PagedResult<Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Users list is not null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UserLifecycle_ManageUser_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var email = $"admin_managed_{Guid.NewGuid()}@example.com";
            var createUserDto = new AdminCreateUserDto
            {
                FullName = "Managed User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            // 1. Create User
            LogInfo($"Creating user {email}.");
            var createResponse = await _api.PostAsync("/api/v1/admin/users", createUserDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");
            var createResult = await _api.LogDeserializeJson<Application.DTOs.User.UserDto>(createResponse);
            var userId = createResult.Data!.Id;

            // 2. Get User By ID
            LogInfo($"Getting user {userId}.");
            var getResponse = await _api.GetAsync($"/api/v1/admin/users/{userId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getResponse.StatusCode, "Get status code is 200 OK");

            // 3. Update User
            var updateDto = new AdminCreateUserDto
            {
                FullName = "Managed User Updated",
                Email = email,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };
            LogInfo($"Updating user {userId}.");
            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}", updateDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");

            // 4. Suspend User (Delete)
            LogInfo($"Suspending user {userId}.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{userId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Suspend status code is 200 OK");

            // 5. Activate User
            LogInfo($"Activating user {userId}.");
            var activateResponse = await _api.PutAsync($"/api/v1/admin/users/{userId}/activate", new { }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, activateResponse.StatusCode, "Activate status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetAuditLogs_ReturnsPagedList()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            LogInfo("Getting audit logs.");
            var response = await _api.GetAsync("/api/v1/admin/audit-log?page=1&pageSize=10", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task GetRoomReports_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            LogInfo("Getting room reports.");
            var response = await _api.GetAsync("/api/v1/admin/room-reports", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }
    }
}