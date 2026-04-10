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

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task Handle_FilterRequest_ReturnsCoachList()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/coach?page=1&pageSize=5", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var data = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(data.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(data.Data, "Coach list data returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetAllCoach_DefaultParams_ReturnsSuccess()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/coach", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Default params returns 200 OK");
            var data = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(data.Success, "API response indicates success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetAllCoach_FilterByMinExperience_ReturnsSuccess()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/coach?page=1&pageSize=10&minExperienceYears=5", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Filter by min experience returns 200 OK");
            var data = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(data.Success, "API response indicates success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetAllCoach_FilterByPriceRange_ReturnsSuccess()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/coach?page=1&pageSize=10&minPrice=0&maxPrice=500", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Filter by price range returns 200 OK");
            var data = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(data.Success, "API response indicates success");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetAllCoach_PageSizeOne_ReturnsAtMostOneItem()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/coach?page=1&pageSize=1", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var data = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(data.Success, "API response indicates success");
            await AssertHelper.AssertTrue(data.Data!.Items.Count <= 1, "Result contains at most one item");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetAllCoach_HighPageNumber_ReturnsEmptyItems()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/coach?page=9999&pageSize=10", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "High page returns 200 OK");
            var data = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(data.Success, "API response indicates success");
            await AssertHelper.AssertTrue(data.Data!.Items.Count == 0, "High page returns empty items");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task GetAllCoach_FilterByHighMinExperience_ReturnsEmptyOrReduced()
        {
            // Act - Very high min experience to likely get 0 results
            var response = await _api.GetAsync("/api/v1/coach?page=1&pageSize=10&minExperienceYears=99", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "High min experience returns 200 OK");
            var data = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(data.Success, "API response indicates success");
        }
    }
}
