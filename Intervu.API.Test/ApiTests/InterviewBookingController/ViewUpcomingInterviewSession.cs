using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    // IC-40
    public class ViewUpcomingInterviewSessionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewUpcomingInterviewSessionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "No dedicated upcoming-session API test exists yet; add when endpoint contract is finalized.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public Task ViewUpcomingInterviewSession_Placeholder() => Task.CompletedTask;
    }
}
