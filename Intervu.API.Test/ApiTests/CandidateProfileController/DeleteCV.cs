using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CandidateProfileController
{
    public class DeleteCVTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public DeleteCVTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Candidate profile does not expose a dedicated Delete CV API in current tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
