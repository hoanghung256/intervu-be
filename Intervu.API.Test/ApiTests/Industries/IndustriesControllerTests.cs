using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Industry;

namespace Intervu.API.Test.ApiTests.Industries
{
    public class IndustriesControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public IndustriesControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Industries")]
        public async Task GetAllIndustries_ReturnsSuccess_WhenDataExists()
        {
            // Act
            LogInfo("Getting all industries.");
            var response = await _api.GetAsync("/api/v1/industries", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");

            var apiResponse = await _api.LogDeserializeJson<PagedResult<IndustryDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data object is not null");
            await AssertHelper.AssertNotEmpty(apiResponse.Data!.Items, "Industries list is not empty (seeded data should exist)");

            // Verify specific seeded data
            var fintech = apiResponse.Data.Items.FirstOrDefault(i => i.Name == "Fintech");
            await AssertHelper.AssertNotNull(fintech, "Seeded industry 'Fintech' exists in the list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Industries")]
        public async Task GetAllIndustries_Pagination_RespectsPageSize()
        {
            // Arrange
            int page = 1;
            int pageSize = 3;

            // Act
            LogInfo($"Getting industries with page={page} and pageSize={pageSize}.");
            var response = await _api.GetAsync($"/api/v1/industries?page={page}&pageSize={pageSize}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");

            var apiResponse = await _api.LogDeserializeJson<PagedResult<IndustryDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertEqual(pageSize, apiResponse.Data!.Items.Count, $"Items count should match pageSize {pageSize}");
            await AssertHelper.AssertEqual(page, apiResponse.Data.CurrentPage, "Current page matches request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Industries")]
        public async Task GetAllIndustries_ReturnsEmptyList_WhenPageIsOutOfRange()
        {
            var response = await _api.GetAsync("/api/v1/industries?page=9999&pageSize=10", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<IndustryDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Data?.Items != null && !apiResponse.Data.Items.Any(), "Items list should be empty");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Industries")]
        public async Task GetAllIndustries_ReturnsBadRequest_WhenPageSizeIsZero()
        {
            var response = await _api.GetAsync("/api/v1/industries?page=1&pageSize=0", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "PageSize 0 should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Industries")]
        public async Task GetAllIndustries_ReturnsBadRequest_WhenPageIsZero()
        {
            var response = await _api.GetAsync("/api/v1/industries?page=0&pageSize=10", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Page 0 should return 400 Bad Request");
        }
    }
}