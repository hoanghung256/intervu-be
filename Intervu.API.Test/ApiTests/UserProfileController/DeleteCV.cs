using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    // IC-16
    public class DeleteCVTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public DeleteCVTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "No dedicated delete-cv API behavior currently covered in existing tests; add once endpoint contract is confirmed.")]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public Task DeleteCV_Placeholder()
        {
            return Task.CompletedTask;
        }
    }
}
