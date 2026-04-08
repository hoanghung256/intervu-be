using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class UpdateUserAccountsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public UpdateUserAccountsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        public async Task Handle_ValidRequest_UpdatesUserAccount()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var email = $"upd_{Guid.NewGuid()}@example.com";
            var create = await _api.PostAsync("/api/v1/admin/users", new AdminCreateUserDto { FullName = "Temp", Email = email, Password = CANDIDATE_PASSWORD, Role = UserRole.Candidate, Status = UserStatus.Active }, jwtToken: token);
            var user = await _api.LogDeserializeJson<Intervu.Application.DTOs.User.UserDto>(create);

            var updateResponse = await _api.PutAsync($"/api/v1/admin/users/{user.Data!.Id}", new AdminCreateUserDto { FullName = "Temp Updated", Email = email, Role = UserRole.Candidate, Status = UserStatus.Active }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");
        }
    }
}
