using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AvailabilitiesController
{
    // IC-30
    public class CreateAvailabilitySlotTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        public CreateAvailabilitySlotTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task CreateAvailabilitySlot_ReturnsSuccess()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(15).AddHours(1));
            var end = start.AddHours(2);

            var createResponse = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status is 200 OK");
            await AssertHelper.AssertTrue(createPayload.Success, "Create availability succeeds");
            await AssertHelper.AssertEqual("Created", createPayload.Message, "Create message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task CreateAvailabilitySlot_Abnormal_InvalidRange_ReturnsBadRequest()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(12));
            var end = start.AddMinutes(-30);

            var response = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid range returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task CreateAvailabilitySlot_Boundary_PastDateTime_ReturnsBadRequest()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(-1));
            var end = start.AddHours(1);

            var response = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Past range returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task CreateAvailabilitySlot_OverlapWithExisting_ReturnsConflict()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(20));
            var end = start.AddHours(2);

            // Create initial slot
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            // Try to create overlapping slot
            var overlapResponse = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start.AddHours(1), TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end.AddHours(1), TimeSpan.Zero)
            }, logBody: true);

            var overlapPayload = await _api.LogDeserializeJson<JsonElement>(overlapResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, overlapResponse.StatusCode, "Status code is 409 Conflict for overlapping slot");
            await AssertHelper.AssertFalse(overlapPayload.Success, "Overlap creation should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task CreateAvailabilitySlot_Boundary_ZeroDuration_ReturnsBadRequest()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(16));

            var response = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(start, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Zero-duration range returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task CreateAvailabilitySlot_Boundary_SameStartAndEndTime_ReturnsBadRequest()
        {
            // Arrange – zero-duration range: start == end
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(15));

            var response = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(start, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Zero-duration range returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task CreateAvailabilitySlot_Abnormal_PastDate_ReturnsBadRequest()
        {
            // Arrange – start time is in the past
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(-5));
            var end = start.AddHours(1);

            var response = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Past-date slot returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_CreateAvailabilitySlot_MinimumValidRange_ReturnsOneBlock()
        {
            // Arrange – exactly 30 minutes → should produce exactly 1 block
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(50).Date.AddHours(9));
            var end = start.AddMinutes(30);

            // Act
            var createResponse = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            // Assert
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Minimum 30-min range returns 200 OK");
            await AssertHelper.AssertTrue(createPayload.Success, "Minimum range create succeeds");
            await AssertHelper.AssertEqual(1, createPayload.Data!.GetProperty("blockCount").GetInt32(), "Exactly 1 block is created for a 30-minute range");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_CreateAvailabilitySlot_LargeRange_ReturnsExpectedBlockCount()
        {
            // Arrange – 4-hour range → should produce 8 blocks (4 * 60 / 30 = 8)
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(52).Date.AddHours(10));
            var end = start.AddHours(4);

            // Act
            var createResponse = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            // Assert
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "4-hour range returns 200 OK");
            await AssertHelper.AssertTrue(createPayload.Success, "Large range create succeeds");
            await AssertHelper.AssertEqual(8, createPayload.Data!.GetProperty("blockCount").GetInt32(), "4-hour range produces exactly 8 blocks");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task CreateAvailabilitySlot_Abnormal_GuidEmptyCoachId_ReturnsNotFound()
        {
            // Arrange – Guid.Empty passes model validation but no coach exists in the DB for it
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(55).Date.AddHours(11));
            var end = start.AddHours(1);

            var response = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = Guid.Empty,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Guid.Empty coach ID returns 404 NotFound");
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
