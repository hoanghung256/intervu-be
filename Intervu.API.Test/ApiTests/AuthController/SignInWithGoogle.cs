using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.Exceptions;
using Intervu.Application.DTOs.User;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    public class SignInWithGoogleTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SignInWithGoogleTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_ReturnsBadRequest_WhenTokenMissing()
        {
            var exception = await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _api.PostAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = "" }, logBody: true));

            await AssertHelper.AssertEqual("IdToken (or credential) is required", exception.Message, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_ReturnsBadRequest_WhenBodyIsNull()
        {
            var exception = await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _api.PostAsync<GoogleLoginRequest?>("/api/v1/auth/google", null, logBody: true));

            await AssertHelper.AssertEqual("IdToken (or credential) is required", exception.Message, "Validation message matches");
        }
    }
}
