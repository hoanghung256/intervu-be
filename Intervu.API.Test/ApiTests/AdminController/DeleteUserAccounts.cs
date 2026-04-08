using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class DeleteUserAccountsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public DeleteUserAccountsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var data = await _api.LogDeserializeJson<LoginResponse>(response);
            return data.Data!.Token;
        }

        [Fact]
        public async Task Handle_ExistingUser_DeletesUserAccount()
        {
            var token = await LoginAdminAsync();
            var create = await _api.PostAsync("/api/v1/admin/users", new AdminCreateUserDto { FullName = "Temp", Email = $"temp_{Guid.NewGuid()}@example.com", Password = CANDIDATE_PASSWORD, Role = UserRole.Candidate, Status = UserStatus.Active }, jwtToken: token);
            var created = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(create);

            var deleteResponse = await _api.DeleteAsync($"/api/v1/admin/users/{created.Data!.Id}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }
    }
}
