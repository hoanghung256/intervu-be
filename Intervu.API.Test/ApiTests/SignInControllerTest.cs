using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests
{
    public class SignInControllerTest : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SignInControllerTest(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Smoke")]
        [Trait("Name", "IC-00")]
        public async Task SignIn_ReturnsDataSuccessfully()
        {
            LoginRequest req = new LoginRequest
            {
                Email = "alice@example.com",
                Password = "123"
            };

            LogInfo("Login.");
            var response = await _api.PostAsync("/api/v1/account/login", req, true);

            LogInfo("Verify json content is not null and not empty");
            var apiResponse = await _api.LogDeserializeJson<LoginResponse>(response);

            await AssertHelper.AssertNotNull(apiResponse.Data, "Json content is not null");
        }
    }
}
