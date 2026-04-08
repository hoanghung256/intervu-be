using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.GeneratedQuestionController
{
    public class DeleteTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public DeleteTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public async Task RejectGeneratedQuestion_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.PutAsync($"/api/v1/generated-questions/{Guid.NewGuid()}/reject", new { }, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }
    }
}
