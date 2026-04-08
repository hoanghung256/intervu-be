using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    public class SignInWithGoogleTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public SignInWithGoogleTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Google sign-in integration requires stable external token fixtures; enable when deterministic test setup is available.")]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public Task GoogleLogin_Placeholder()
        {
            return Task.CompletedTask;
        }
    }
}
