using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class DevicePreCheckTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public DevicePreCheckTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Device pre-check API is not present in current test suite.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public Task DevicePreCheck_Placeholder() => Task.CompletedTask;
    }
}
