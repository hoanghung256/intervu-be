using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class ResolveInterviewBookingFinanceIssueTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ResolveInterviewBookingFinanceIssueTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "No explicit finance-issue endpoint is covered in existing tests; add once API contract is available.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public Task ResolveInterviewBookingFinanceIssue_Placeholder() => Task.CompletedTask;
    }
}
