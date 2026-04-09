using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class ViewRevenueStatisticsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        // IC-56
        public ViewRevenueStatisticsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Revenue statistics endpoint is not explicitly covered in current tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public Task ViewRevenueStatistics_Placeholder() => Task.CompletedTask;
    }
}
