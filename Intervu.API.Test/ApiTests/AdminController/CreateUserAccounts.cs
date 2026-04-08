using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Admin;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class CreateUserAccountsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public CreateUserAccountsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var data = await _api.LogDeserializeJson<LoginResponse>(response);
            return data.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task Handle_ValidRequest_CreatesUserAccount()
        {
            var token = await LoginAdminAsync();
            var email = $"admin_created_{Guid.NewGuid()}@example.com";
            var response = await _api.PostAsync("/api/v1/admin/users", new AdminCreateUserDto { FullName = "Managed User", Email = email, Password = CANDIDATE_PASSWORD, Role = UserRole.Candidate, Status = UserStatus.Active }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Create status code is 200 OK");
        }
    }
}
