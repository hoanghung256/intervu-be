using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.GeneratedQuestionController
{
    public class CreateTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public CreateTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Test case creation API is not explicitly covered in current backend tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public Task CreateTestCases_Placeholder() => Task.CompletedTask;
    }
}
