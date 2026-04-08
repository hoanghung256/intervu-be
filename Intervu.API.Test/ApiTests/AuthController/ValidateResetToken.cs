using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    public class ValidateResetTokenTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ValidateResetTokenTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ValidateResetToken_ReturnsBadRequest_WhenTokenIsInvalid()
        {
            LogInfo("Validating a fake reset token.");
            var response = await _api.GetAsync("/api/v1/auth/validate-reset-token/fake-token-123", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");

            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Validation failed");
        }
    }
}
