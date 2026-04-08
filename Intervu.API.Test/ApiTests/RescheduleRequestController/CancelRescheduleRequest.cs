using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    public class CancelRescheduleRequestTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public CancelRescheduleRequestTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "No explicit cancel-reschedule endpoint exists in current backend API tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
