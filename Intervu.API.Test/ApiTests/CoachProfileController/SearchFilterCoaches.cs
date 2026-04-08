using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.Common;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CoachProfileController
{
    public class SearchFilterCoachesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public SearchFilterCoachesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        public async Task Handle_FilterRequest_ReturnsCoachList()
        {
            var response = await _api.GetAsync("/api/v1/coach?page=1&pageSize=5", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var data = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertNotNull(data.Data, "Coach list data returned");
        }
    }
}
