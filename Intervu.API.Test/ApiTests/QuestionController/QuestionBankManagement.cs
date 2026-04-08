using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class QuestionBankManagementTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public QuestionBankManagementTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Dedicated endpoints for contribute/update/delete question bank are not explicitly covered in current controller tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public Task QuestionBankManagement_Placeholder() => Task.CompletedTask;
    }
}
