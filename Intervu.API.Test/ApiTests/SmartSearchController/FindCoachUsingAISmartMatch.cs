using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.SmartSearch;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.SmartSearchController
{
    public class FindCoachUsingAISmartMatchTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public FindCoachUsingAISmartMatchTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "SmartSearch")]
        public async Task FindCoachUsingAISmartMatch_ValidQuery_ReturnsOk()
        {
            var response = await _api.PostAsync("/api/v1/smart-search/coaches", new SmartSearchRequest
            {
                Query = "Find a backend coach experienced in .NET and system design",
                TopK = 3
            }, logBody: true);

            var isExpected = response.StatusCode == HttpStatusCode.OK;
            await AssertHelper.AssertTrue(isExpected, "Valid query returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "SmartSearch")]
        public async Task FindCoachUsingAISmartMatch_QueryEmptyAndContextEmpty_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/smart-search/coaches", new SmartSearchRequest
            {
                Query = "",
                ExtractedProfileContext = null,
                TopK = 3
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Empty query and context returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "SmartSearch")]
        public async Task FindCoachUsingAISmartMatch_WhitespaceQueryAndContextEmpty_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/smart-search/coaches", new SmartSearchRequest
            {
                Query = "   ",
                ExtractedProfileContext = "",
                TopK = 5
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Whitespace query and empty context returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "SmartSearch")]
        public async Task FindCoachUsingAISmartMatch_ContextOnly_ReturnsOk()
        {
            var response = await _api.PostAsync("/api/v1/smart-search/coaches", new SmartSearchRequest
            {
                Query = "",
                ExtractedProfileContext = "{\"cv\":{\"skills\":[\"C#\",\"SQL\"]}}",
                TopK = 5
            }, logBody: true);

            var isExpected = response.StatusCode == HttpStatusCode.OK;
            await AssertHelper.AssertTrue(isExpected, "Context-only request returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "SmartSearch")]
        public async Task FindCoachUsingAISmartMatch_WithoutBody_ReturnsUnsupportedMediaType()
        {
            var response = await _api.PostAsync<object>("/api/v1/smart-search/coaches", null, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.UnsupportedMediaType, response.StatusCode, "Missing request body returns 415 Unsupported Media Type");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "SmartSearch")]
        public async Task FindCoachUsingAISmartMatch_TopKOne_ReturnsOk()
        {
            var response = await _api.PostAsync("/api/v1/smart-search/coaches", new SmartSearchRequest
            {
                Query = "Find data engineer coach",
                TopK = 1
            }, logBody: true);

            var isExpected = response.StatusCode == HttpStatusCode.OK;
            await AssertHelper.AssertTrue(isExpected, "TopK=1 returns 200 OK");
        }
    }
}

