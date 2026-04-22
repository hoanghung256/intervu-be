using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AvailabilitiesController
{
    // TODO: Add UC into doc - Update availability slot
    public class UpdateAvailabilitySlotTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        public UpdateAvailabilitySlotTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_ReturnsSuccess()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(73).Date.AddHours(4));
            var end = start.AddHours(2);

            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            var updateResponse = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(start.AddMinutes(30), TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(end.AddMinutes(30), TimeSpan.Zero)
            }, logBody: true);

            var updatePayload = await _api.LogDeserializeJson<JsonElement>(updateResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status is 200 OK");
            await AssertHelper.AssertTrue(updatePayload.Success, "Update availability succeeds");
            await AssertHelper.AssertEqual("Updated", updatePayload.Message, "Update message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_Abnormal_NewEndBeforeNewStart_ReturnsBadRequest()
        {
            // Arrange – create a valid slot first
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(85).Date.AddHours(6));
            var end = start.AddHours(2);
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            var response = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(start, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Inverted new time range returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_Boundary_SameNewStartAndEnd_ReturnsBadRequest()
        {
            // Arrange
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(90).Date.AddHours(7));
            var end = start.AddHours(1);
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            var response = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(start, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Zero-duration new range returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_Abnormal_NonExistentOriginalSlot_ReturnsNotFound()
        {
            // Arrange – a time range that was never created (no availability blocks exist for it)
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(102).Date.AddHours(8));
            var end = start.AddHours(1);

            var response = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(start.AddHours(2), TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(end.AddHours(2), TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent original slot returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_Abnormal_GuidEmptyCoachId_ReturnsNotFound()
        {
            // Arrange – Guid.Empty passes model validation but coach does not exist in the DB
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(108).Date.AddHours(9));
            var end = start.AddHours(1);

            var response = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = Guid.Empty,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(start.AddHours(1), TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(end.AddHours(1), TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Guid.Empty coach ID returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_NonExistentOriginalRange_ReturnsNotFound()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(90));
            var end = start.AddHours(1);

            var updateResponse = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(start.AddHours(1), TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(end.AddHours(1), TimeSpan.Zero)
            }, logBody: true);

            var updatePayload = await _api.LogDeserializeJson<JsonElement>(updateResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, updateResponse.StatusCode, "Status code is 404 Not Found for non-existent original range");
            await AssertHelper.AssertFalse(updatePayload.Success, "Update should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_Boundary_InvalidNewRange_ReturnsBadRequest()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(91));
            var end = start.AddHours(1);

            var response = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(start, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid new range returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_NewRangeOverlapWithOther_ReturnsConflict()
        {
            var start1 = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(92));
            var end1 = start1.AddHours(1);
            var start2 = end1.AddHours(1);
            var end2 = start2.AddHours(1);

            // Create two slots
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start1, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end1, TimeSpan.Zero)
            });
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start2, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end2, TimeSpan.Zero)
            });

            // Try to update first slot to overlap second slot
            var updateResponse = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start1, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end1, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(start2.AddMinutes(-30), TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(start2.AddMinutes(30), TimeSpan.Zero)
            }, logBody: true);

            var updatePayload = await _api.LogDeserializeJson<JsonElement>(updateResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, updateResponse.StatusCode, "Status code is 409 Conflict for overlapping new range");
            await AssertHelper.AssertFalse(updatePayload.Success, "Update should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task UpdateAvailabilitySlot_PastOriginalRange_ReturnsNotFound()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(-5));
            var end = start.AddHours(1);
            var newStart = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(10).Date.AddHours(10));
            var newEnd = newStart.AddHours(1);

            var updateResponse = await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(newStart, TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(newEnd, TimeSpan.Zero)
            }, logBody: true);

            var updatePayload = await _api.LogDeserializeJson<JsonElement>(updateResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, updateResponse.StatusCode, "Status code is 404 Not Found for past original range");
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
