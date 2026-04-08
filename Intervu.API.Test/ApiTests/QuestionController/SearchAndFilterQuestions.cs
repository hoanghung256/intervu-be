using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Question;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class SearchAndFilterQuestionsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SearchAndFilterQuestionsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task Search_ReturnsSuccess_WhenKeywordProvided()
        {
            var response = await _api.GetAsync("/api/v1/questions/search?keyword=Searchable", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<List<QuestionSearchResultDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }
    }
}
