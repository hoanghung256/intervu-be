using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.GeneratedQuestionController
{
    public class DeleteTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public DeleteTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Test case deletion API is not explicitly covered in current backend tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public Task DeleteTestCases_Placeholder() => Task.CompletedTask;
    }
}
