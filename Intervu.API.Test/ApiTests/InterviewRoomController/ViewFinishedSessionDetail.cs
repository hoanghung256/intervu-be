using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class ViewFinishedSessionDetailTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewFinishedSessionDetailTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Finished session detail endpoint is not explicitly covered in current InterviewRoom tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
