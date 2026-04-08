using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class UpdateQuestionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public UpdateQuestionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) { }

        [Fact(Skip = "Update question endpoint is not explicitly covered in current QuestionController tests.")]
        public Task Handle_Placeholder_NotImplemented() => Task.CompletedTask;
    }
}
