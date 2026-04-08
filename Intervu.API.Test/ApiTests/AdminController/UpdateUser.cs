using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class UpdateUserTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public UpdateUserTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task UpdateUser_ReturnsSuccess()
        {
            var token = await LoginAdminAsync();
            var email = $"admin_updated_{Guid.NewGuid()}@example.com";
            var createUserDto = new AdminCreateUserDto
            {
                FullName = "Managed User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            LogInfo($"Creating user {email} for update flow.");
            var createResponse = await _api.PostAsync("/api/v1/admin/users", createUserDto, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");
            var createResult = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(createResponse);
            var userId = createResult.Data!.Id;

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
        }
    }
}