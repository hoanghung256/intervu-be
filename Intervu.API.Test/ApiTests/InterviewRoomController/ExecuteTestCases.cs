using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class ExecuteTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ExecuteTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Execute test cases API under InterviewRoom is not present in current backend tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
