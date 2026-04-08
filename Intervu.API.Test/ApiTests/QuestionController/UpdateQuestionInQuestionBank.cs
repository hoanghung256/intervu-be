using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Question;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class UpdateQuestionInQuestionBankTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public UpdateQuestionInQuestionBankTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task UpdateQuestionInQuestionBank_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.PutAsync($"/api/v1/questions/{Guid.NewGuid()}", new UpdateQuestionRequest
            {
                Title = "Unauthorized Update",
                Content = "Unauthorized Update",
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
