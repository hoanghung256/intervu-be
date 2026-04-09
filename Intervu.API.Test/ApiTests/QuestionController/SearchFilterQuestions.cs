using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Question;
using System.Net;
using System.Text.Json;
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

        [Fact]
        public async Task Handle_EmptyKeyword_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/questions/search?keyword=", logBody: true);
            var content = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest for empty keyword");
            await AssertHelper.AssertContains("keyword", content, "Validation message contains keyword field");
        }

        [Fact]
        public async Task Handle_SearchBySpecificCompany_ReturnsFilteredResults()
        {
            var googleId = "11111111-1111-4111-8111-111111111111";
            var response = await _api.GetAsync($"/api/v1/questions/search?keyword=test&companyIds={googleId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for filtered search");
            var body = await _api.LogDeserializeJson<List<QuestionSearchResultDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "Filtered search succeeds");
        }

        [Fact]
        public async Task Handle_SearchByInvalidCategory_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/questions/search?keyword=test&category=999", logBody: true); // Invalid category
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid category");
        }

        [Fact]
        public async Task Handle_SearchWithSpecialCharacters_ReturnsSuccess()
        {
            var response = await _api.GetAsync("/api/v1/questions/search?keyword=c%2B%2B%23", logBody: true); // Searching for "c++#"
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for special characters");
            var body = await _api.LogDeserializeJson<List<QuestionSearchResultDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "Search with special characters succeeds");
        }

        [Fact]
        public async Task Handle_SearchNoResults_ReturnsEmptyList()
        {
            var response = await _api.GetAsync("/api/v1/questions/search?keyword=nonexistentquestionkeywordxyz", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for no results");
            var body = await _api.LogDeserializeJson<List<QuestionSearchResultDto>>(response);
            await AssertHelper.AssertEqual(0, body.Data?.Count, "Returned search results list should be empty");
        }
    }
}
