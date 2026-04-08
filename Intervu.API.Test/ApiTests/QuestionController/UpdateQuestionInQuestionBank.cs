using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class UpdateQuestionInQuestionBankTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public UpdateQuestionInQuestionBankTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Update question endpoint coverage is not present in current tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public Task UpdateQuestionInQuestionBank_Placeholder() => Task.CompletedTask;
    }
}
