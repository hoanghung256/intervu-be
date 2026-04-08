using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class ContributeToQuestionBankTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ContributeToQuestionBankTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Contribution endpoint is currently tested via interview experience flow; add direct endpoint tests when exposed.")]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public Task ContributeToQuestionBank_Placeholder() => Task.CompletedTask;
    }
}
