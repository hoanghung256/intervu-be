using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Skill;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Skills
{
    public class SkillsControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SkillsControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Skills")]
        public async Task GetAllSkills_ReturnsSuccessAndData_WhenParametersAreValid()
        {            
            int page = 1;
            int pageSize = 10;
            
            LogInfo("Getting all skills with valid pagination.");
            var response = await _api.GetAsync($"/api/v1/skills?page={page}&pageSize={pageSize}", logBody: true);
            
            LogInfo("Verify response.");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<SkillDto>>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Data items are not null");
            
            // Assuming seed data exists
            if (apiResponse.Data?.Items != null)
            {
                await AssertHelper.AssertNotEmpty(apiResponse.Data.Items, "Data items are not empty");
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Skills")]
        public async Task GetAllSkills_ReturnsEmptyList_WhenPageIsOutOfRange()
        {
            // Arrange
            int page = 9999;
            int pageSize = 10;

            // Act
            LogInfo($"Getting skills for page {page}.");
            var response = await _api.GetAsync($"/api/v1/skills?page={page}&pageSize={pageSize}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<SkillDto>>(response);
            
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertTrue(apiResponse.Data?.Items != null && !apiResponse.Data.Items.Any(), "Items list should be empty");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Skills")]
        public async Task GetAllSkills_ReturnsSuccess_WhenPageSizeIsLarge()
        {
            // Arrange
            int page = 1;
            int pageSize = 100; // Boundary/Large value

            // Act
            LogInfo($"Getting skills with large page size {pageSize}.");
            var response = await _api.GetAsync($"/api/v1/skills?page={page}&pageSize={pageSize}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<SkillDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
        }
    }
}