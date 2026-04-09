using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    // IC-43: realtime function, can not test via unit test
    public class UseLiveCodingEditorTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public UseLiveCodingEditorTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Live coding editor API flows are not currently covered in backend API tests.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public Task UseLiveCodingEditor_Placeholder() => Task.CompletedTask;
    }
}
