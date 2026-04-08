using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class CreateUserTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CreateUserTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task CreateUser_ReturnsSuccess()
        {
            var token = await LoginAdminAsync();
            var email = $"admin_created_{Guid.NewGuid()}@example.com";
            var createUserDto = new AdminCreateUserDto
            {
                FullName = "Managed User",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = UserRole.Candidate,
                Status = UserStatus.Active
            };

            LogInfo($"Creating user {email}.");
            var createResponse = await _api.PostAsync("/api/v1/admin/users", createUserDto, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");
            var createResult = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(createResponse);
            await AssertHelper.AssertNotNull(createResult.Data, "Created user data is not null");
            await AssertHelper.AssertEqual(email, createResult.Data!.Email, "Created user email matches request");
        }
    }
}