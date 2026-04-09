using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Company;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Companies
{
    public class CompaniesControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CompaniesControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Companies")]
        public async Task GetAllCompanies_ReturnsSuccessAndData()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;

            // Act
            LogInfo($"Getting all companies with page {page} and pageSize {pageSize}.");
            var response = await _api.GetAsync($"/api/v1/companies?page={page}&pageSize={pageSize}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");

            var apiResponse = await _api.LogDeserializeJson<PagedResult<CompanyDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data object should not be null");
            await AssertHelper.AssertNotNull(apiResponse.Data!.Items, "Items list should not be null");
            await AssertHelper.AssertNotEmpty(apiResponse.Data.Items, "Items list should not be empty (seeded data exists)");

            // Verify specific seeded data (e.g., Google)
            var google = apiResponse.Data.Items.FirstOrDefault(c => c.Name == "Google");
            await AssertHelper.AssertNotNull(google, "Seeded company 'Google' should exist in the response");
            await AssertHelper.AssertEqual("https://google.com", google!.Website, "Website matches seeded data");
            
            // Verify pagination metadata if available
            if (apiResponse.Data.TotalItems > 0)
            {
                await AssertHelper.AssertTrue(apiResponse.Data.TotalItems >= apiResponse.Data.Items.Count, "Total count should be greater than or equal to items count");
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Companies")]
        public async Task GetAllCompanies_ReturnsEmptyList_WhenPageIsOutOfRange()
        {
            var response = await _api.GetAsync("/api/v1/companies?page=9999&pageSize=10", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CompanyDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Data?.Items != null && !apiResponse.Data.Items.Any(), "Items list should be empty");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Companies")]
        public async Task GetAllCompanies_ReturnsBadRequest_WhenPageSizeIsZero()
        {
            var response = await _api.GetAsync("/api/v1/companies?page=1&pageSize=0", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "PageSize 0 should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Companies")]
        public async Task GetAllCompanies_ReturnsBadRequest_WhenPageIsZero()
        {
            var response = await _api.GetAsync("/api/v1/companies?page=0&pageSize=10", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Page 0 should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Companies")]
        public async Task GetAllCompanies_ReturnsSuccess_WhenPageSizeIsLarge()
        {
            var response = await _api.GetAsync("/api/v1/companies?page=1&pageSize=100", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CompanyDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
        }
    }
}