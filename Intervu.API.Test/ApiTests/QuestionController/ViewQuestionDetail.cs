using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class ViewQuestionDetailTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewQuestionDetailTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Question detail test is currently commented out in legacy suite; enable when stable fixture data is finalized.")]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public Task ViewQuestionDetail_Placeholder() => Task.CompletedTask;
    }
}
