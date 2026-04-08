using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class InterveneScheduleTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public InterveneScheduleTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Intervene-schedule endpoint is not covered in current backend tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public Task InterveneSchedule_Placeholder() => Task.CompletedTask;
    }
}
