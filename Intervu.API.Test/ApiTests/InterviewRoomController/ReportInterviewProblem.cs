using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewRoom;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class ReportInterviewProblemTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ReportInterviewProblemTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "Covered in legacy room tests; extracted endpoint behavior requires room-setup helpers to be shared before enabling.")]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public Task ReportInterviewProblem_Placeholder() => Task.CompletedTask;
    }
}
