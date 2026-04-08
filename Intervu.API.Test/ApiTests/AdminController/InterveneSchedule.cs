using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class InterveneScheduleTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public InterveneScheduleTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Intervene schedule endpoint is not explicitly covered in current API tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
