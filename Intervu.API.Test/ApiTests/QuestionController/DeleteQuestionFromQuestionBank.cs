using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class DeleteQuestionFromQuestionBankTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public DeleteQuestionFromQuestionBankTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Delete question endpoint coverage is not present in current tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public Task DeleteQuestionFromQuestionBank_Placeholder() => Task.CompletedTask;
    }
}
