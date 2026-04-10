using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.GeneratedQuestionController
{
    public class ExecuteTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ExecuteTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public async Task ApproveGeneratedQuestion_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.PutAsync($"/api/v1/generated-questions/{Guid.NewGuid()}/approve", new ApproveGeneratedQuestionRequest
            {
                Title = "Unauthorized Approve",
                Content = "Unauthorized Approve",
                Level = ExperienceLevel.Junior,
                Round = InterviewRound.TechnicalScreen,
                Category = QuestionCategory.Coding,
                CompanyIds = new List<Guid>(),
                Roles = new List<Role>(),
                TagIds = new List<Guid>()
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }
    }
}
