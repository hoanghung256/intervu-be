using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class ViewRevenueStatisticsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewRevenueStatisticsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Revenue statistics API is not explicitly covered in current Admin controller tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
