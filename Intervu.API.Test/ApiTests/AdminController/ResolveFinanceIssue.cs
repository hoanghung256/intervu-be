using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class ResolveFinanceIssueTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ResolveFinanceIssueTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Resolve finance issue endpoint is not explicitly covered in current API tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
