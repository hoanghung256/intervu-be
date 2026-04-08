using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class ViewUserAccountsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public ViewUserAccountsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        public async Task Handle_AdminRequest_ReturnsPagedUsers()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var data = await _api.LogDeserializeJson<LoginResponse>(login);
            var response = await _api.GetAsync("/api/v1/admin/users?page=1&pageSize=5", jwtToken: data.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var body = await _api.LogDeserializeJson<PagedResult<Intervu.Application.DTOs.Admin.UserDto>>(response);
            await AssertHelper.AssertNotNull(body.Data?.Items, "Users list is not null");
        }
    }
}
