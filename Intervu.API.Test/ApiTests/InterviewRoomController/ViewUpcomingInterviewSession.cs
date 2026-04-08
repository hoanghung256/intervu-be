using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class ViewUpcomingInterviewSessionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewUpcomingInterviewSessionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Upcoming-session endpoint is not explicitly exposed under InterviewRoom in current tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
