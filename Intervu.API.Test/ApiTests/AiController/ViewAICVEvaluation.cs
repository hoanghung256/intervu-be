using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AiController
{
    public class ViewAICVEvaluationTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewAICVEvaluationTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "AI CV evaluation endpoint is not covered in current backend API test suite.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
