using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewRoom;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class ReportInterviewProblemTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ReportInterviewProblemTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ReportInterviewProblem_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.PostAsync($"/api/v1/interviewroom/{Guid.NewGuid()}/report", new CreateRoomReportRequest
            {
                Reason = "Audio issue",
                Details = "Mic was disconnected during interview"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }
    }
}
