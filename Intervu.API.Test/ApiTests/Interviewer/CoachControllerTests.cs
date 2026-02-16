using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.Common;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Interviewer
{
    public class CoachControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CoachControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_ReturnsSuccessAndList()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;

            // Act
            LogInfo($"Getting all coaches with page {page} and pageSize {pageSize}.");
            var response = await _api.GetAsync($"/api/v1/coach?page={page}&pageSize={pageSize}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");

            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data object should not be null");
            await AssertHelper.AssertNotEmpty(apiResponse.Data!.Items, "Items list should not be empty (seeded coaches exist)");

            // Verify seeded Bob exists
            var bob = apiResponse.Data.Items.FirstOrDefault(c => c.User.FullName.Contains("Bob"));
            await AssertHelper.AssertNotNull(bob, "Seeded coach 'Bob' should exist in the list");
        }
    }
}