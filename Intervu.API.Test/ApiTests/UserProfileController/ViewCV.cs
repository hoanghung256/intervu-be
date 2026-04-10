using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    // IC-13
    public class ViewCVTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewCVTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "No dedicated view-cv API behavior currently covered in existing tests; add once endpoint contract is confirmed.")]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public Task ViewCV_Placeholder()
        {
            return Task.CompletedTask;
        }
    }
}
