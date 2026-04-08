using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class ViewFinishedInterviewSessionDetailTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewFinishedInterviewSessionDetailTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Finished-session detail endpoint coverage is not explicitly present in current tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public Task ViewFinishedInterviewSessionDetail_Placeholder() => Task.CompletedTask;
    }
}
