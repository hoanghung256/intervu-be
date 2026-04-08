using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class ViewPaymentHistoryTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewPaymentHistoryTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Payment history endpoint is not explicitly covered in current tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public Task ViewPaymentHistory_Placeholder() => Task.CompletedTask;
    }
}
