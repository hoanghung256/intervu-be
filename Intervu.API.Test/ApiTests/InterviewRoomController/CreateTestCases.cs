using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class CreateTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public CreateTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Create test cases API under InterviewRoom is not present in current backend tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
