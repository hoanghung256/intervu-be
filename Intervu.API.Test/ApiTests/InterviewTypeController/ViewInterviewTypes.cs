using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
using System.Text.Json;
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

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task GetAllInterviewTypes_InvalidPageSize_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/interviewtype?pageSize=0", logBody: true); // Assuming pageSize=0 is invalid
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for pageSize=0");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task GetAllInterviewTypes_LargePageSize_ReturnsLimitedResults()
        {
            var response = await _api.GetAsync("/api/v1/interviewtype?pageSize=1000", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for large pageSize");
            var getAllData = await _api.LogDeserializeJson<PagedResult<InterviewTypeDto>>(response);
            await AssertHelper.AssertNotNull(getAllData.Data, "Data should not be null");
            // Assuming max pageSize is 100
            await AssertHelper.AssertTrue(getAllData.Data?.Items?.Count <= 100, "Returned items count should be within API's max pageSize limit");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task GetById_InvalidFormatId_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/interviewtype/invalid-id-format", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid format ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task GetById_ValidId_ReturnsSuccess()
        {
            // First, find an existing ID
            var getAllResponse = await _api.GetAsync("/api/v1/interviewtype?pageSize=1", logBody: true);
            var getAllData = await _api.LogDeserializeJson<PagedResult<InterviewTypeDto>>(getAllResponse);
            if (getAllData.Data?.Items?.Count > 0)
            {
                var id = getAllData.Data.Items[0].Id;
                var response = await _api.GetAsync($"/api/v1/interviewtype/{id}", logBody: true);
                var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

                await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Get by ID status code is 200 OK");
                await AssertHelper.AssertTrue(payload.Success, "Get by ID request succeeds");
                await AssertHelper.AssertNotNull(payload.Data, "Interview type data is returned");
            }
        }
    }
}
