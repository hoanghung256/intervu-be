using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Question;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class SearchFilterQuestionsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public SearchFilterQuestionsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        public async Task Handle_KeywordSearch_ReturnsSuccess()
        {
            var response = await _api.GetAsync("/api/v1/questions/search?keyword=test", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var body = await _api.LogDeserializeJson<List<QuestionSearchResultDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "Search succeeds");
        }
    }
}
