using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class DeleteQuestionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public DeleteQuestionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Delete question endpoint is not explicitly covered in current QuestionController tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
