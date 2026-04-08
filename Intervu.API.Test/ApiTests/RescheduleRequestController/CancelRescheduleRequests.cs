using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    public class CancelRescheduleRequestsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public CancelRescheduleRequestsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "No explicit cancel-reschedule endpoint test exists in current suite; add once route contract is finalized.")]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public Task CancelRescheduleRequests_Placeholder() => Task.CompletedTask;
    }
}
