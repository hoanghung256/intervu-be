using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class ViewAllPaymentHistoryTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewAllPaymentHistoryTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Admin payment history endpoint is not explicitly covered in current API tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
