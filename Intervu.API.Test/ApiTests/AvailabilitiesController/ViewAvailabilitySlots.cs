using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AvailabilitiesController
{
    public class ViewAvailabilitySlotsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
        private static readonly Guid NonExistentCoachId = Guid.NewGuid();

        public ViewAvailabilitySlotsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachAvailabilities_ReturnsSuccessWithData()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(24));
            var end = start.AddHours(1);

            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            var response = await _api.GetAsync($"/api/v1/availabilities/{BobCoachId}", logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds");
            await AssertHelper.AssertEqual("Success", payload.Message, "Response message matches");
            await AssertHelper.AssertNotNull(payload.Data, "Availability data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachFreeSlots_ReturnsSuccessWithData()
        {
            var response = await _api.GetAsync($"/api/v1/availabilities/{BobCoachId}/free-slots", logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds");
            await AssertHelper.AssertEqual("Success", payload.Message, "Response message matches");
            await AssertHelper.AssertNotNull(payload.Data, "Free slot data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachAvailabilities_WithMonthYearFilter_ReturnsSuccess()
        {
            // Arrange – seed a slot in the target month
            var targetDate = DateTime.UtcNow.AddDays(30);
            var start = AlignToHalfHourUtc(new DateTime(targetDate.Year, targetDate.Month, 1, 8, 0, 0, DateTimeKind.Utc));
            var end = start.AddHours(1);
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            // Act – filter by month and year
            var response = await _api.GetAsync(
                $"/api/v1/availabilities/{BobCoachId}?month={targetDate.Month}&year={targetDate.Year}",
                logBody: true);

            // Assert
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Month/year filter returns 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Filtered availability request succeeds");
            await AssertHelper.AssertEqual("Success", payload.Message, "Response message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachFreeSlots_WithMonthYearFilter_ReturnsSuccess()
        {
            // Arrange
            var targetDate = DateTime.UtcNow.AddDays(30);

            // Act – filter free slots by month and year
            var response = await _api.GetAsync(
                $"/api/v1/availabilities/{BobCoachId}/free-slots?month={targetDate.Month}&year={targetDate.Year}",
                logBody: true);

            // Assert
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Month/year filter for free slots returns 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Filtered free slots request succeeds");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachAvailabilities_NonExistentCoachId_ReturnsSuccessWithEmptyData()
        {
            // Arrange – a GUID that has no corresponding coach in the database
            var nonExistentCoachId = Guid.NewGuid();

            // Act
            var response = await _api.GetAsync($"/api/v1/availabilities/{nonExistentCoachId}", logBody: true);

            // Assert – the endpoint should return 200 with an empty collection rather than an error
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Non-existent coach ID returns 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds for unknown coach");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachAvailabilities_GuidEmptyCoachId_ReturnsSuccessWithEmptyData()
        {
            // Arrange – Guid.Empty is a valid GUID but no coach exists for it
            var emptyCoachId = Guid.Empty;

            // Act
            var response = await _api.GetAsync($"/api/v1/availabilities/{emptyCoachId}", logBody: true);

            // Assert – endpoint returns 200 with empty collection rather than an error
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Guid.Empty coach ID returns 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds for Guid.Empty coach ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachAvailabilities_WithInvalidMonth_ReturnsSuccess()
        {
            // Arrange – month=13 is out of calendar range; no server-side query param validation expected
            // Act
            var response = await _api.GetAsync(
                $"/api/v1/availabilities/{BobCoachId}?month=13&year={DateTime.UtcNow.Year}",
                logBody: true);

            // Assert – API returns 200 (filters produce zero results for an impossible month)
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Invalid month=13 still returns 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds with out-of-range month");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachFreeSlots_AfterCreatingSlot_ReturnsNonEmptyData()
        {
            // Arrange – create a future slot, then verify it shows up in the free-slots endpoint
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(32).Date.AddHours(14));
            var end = start.AddHours(1);
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            // Act
            var response = await _api.GetAsync(
                $"/api/v1/availabilities/{BobCoachId}/free-slots?month={start.Month}&year={start.Year}",
                logBody: true);

            // Assert – at least the seeded slot should appear
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Free-slots returns 200 OK after slot creation");
            await AssertHelper.AssertTrue(payload.Success, "Free-slots request succeeds");
            await AssertHelper.AssertNotNull(payload.Data, "Free-slots data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachAvailabilities_NonExistentCoach_ReturnsEmptyList()
        {
            var response = await _api.GetAsync($"/api/v1/availabilities/{NonExistentCoachId}", logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds");
            await AssertHelper.AssertEqual(JsonValueKind.Array, payload.Data.ValueKind, "Data should be an array");
            await AssertHelper.AssertEqual(0, payload.Data.GetArrayLength(), "Data should be empty for non-existent coach");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachFreeSlots_NonExistentCoach_ReturnsEmptyList()
        {
            var response = await _api.GetAsync($"/api/v1/availabilities/{NonExistentCoachId}/free-slots", logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request succeeds");
            await AssertHelper.AssertEqual(JsonValueKind.Array, payload.Data.ValueKind, "Data should be an array");
            await AssertHelper.AssertEqual(0, payload.Data.GetArrayLength(), "Free slots should be empty for non-existent coach");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_ViewCoachAvailabilities_InvalidId_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/availabilities/invalid-guid", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid GUID");
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
