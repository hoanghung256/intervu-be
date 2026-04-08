using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class ModifyTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ModifyTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Modify test cases API under InterviewRoom is not present in current backend tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
