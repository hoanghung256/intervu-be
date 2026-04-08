using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.GeneratedQuestionController
{
    public class ExecuteTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ExecuteTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Test case execution API is not explicitly covered in current backend tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public Task ExecuteTestCases_Placeholder() => Task.CompletedTask;
    }
}
