using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class DeleteQuestionFromQuestionBankTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public DeleteQuestionFromQuestionBankTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task DeleteQuestionFromQuestionBank_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.DeleteAsync($"/api/v1/questions/{Guid.NewGuid()}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }
    }
}
