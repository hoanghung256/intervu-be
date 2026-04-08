using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class ViewQuestionDetailTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewQuestionDetailTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetDetail_ReturnsNotFound_WhenQuestionDoesNotExist()
        {
            var response = await _api.GetAsync($"/api/v1/questions/{Guid.NewGuid()}", logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 NotFound");
            await AssertHelper.AssertFalse(apiResponse.Success, "Response indicates failure");
            await AssertHelper.AssertEqual("Question not found", apiResponse.Message, "Error message matches");
        }
    }
}
