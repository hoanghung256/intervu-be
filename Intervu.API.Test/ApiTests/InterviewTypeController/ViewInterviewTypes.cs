using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewTypeController
{
    public class ViewInterviewTypesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewInterviewTypesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task GetAllInterviewTypes_ReturnsSuccess()
        {
            var response = await _api.GetAsync("/api/v1/interviewtype?pageSize=20", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Get all status code is 200 OK");
            var getAllData = await _api.LogDeserializeJson<PagedResult<InterviewTypeDto>>(response);
            await AssertHelper.AssertNotNull(getAllData.Data, "Data should not be null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task GetById_ReturnsNotFound_WhenIdIsInvalid()
        {
            var invalidId = Guid.NewGuid();
            var response = await _api.GetAsync($"/api/v1/interviewtype/{invalidId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
        }
    }
}
