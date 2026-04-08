using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CandidateProfileController
{
    public class ViewCVTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewCVTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Candidate profile does not expose a dedicated View CV API in current tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
