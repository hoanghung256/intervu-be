using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.GeneratedQuestionController
{
    public class ModifyTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ModifyTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Test case modification API is not explicitly covered in current backend tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public Task ModifyTestCases_Placeholder() => Task.CompletedTask;
    }
}
