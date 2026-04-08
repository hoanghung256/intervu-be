using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AvailabilitiesController
{
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
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(12));
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
        public async Task CreateAvailabilitySlot_ThrowsArgumentException_WhenRangeIsInvalid()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(12));
            var end = start.AddMinutes(-30);

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
                {
                    CoachId = BobCoachId,
                    RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
                }, logBody: true));

            await AssertHelper.AssertContains("RangeEndTime must be greater than RangeStartTime", exception.Message, "Validation exception message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_CreateAvailabilitySlot_SameStartAndEndTime_ThrowsArgumentException()
        {
            // Arrange – zero-duration range: start == end
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(15));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
                {
                    CoachId = BobCoachId,
                    RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    RangeEndTime = new DateTimeOffset(start, TimeSpan.Zero)
                }, logBody: true));

            await AssertHelper.AssertContains("RangeEndTime must be greater than RangeStartTime", exception.Message, "Zero-duration range is rejected");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_CreateAvailabilitySlot_PastDate_ThrowsArgumentException()
        {
            // Arrange – start time is in the past
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(-5));
            var end = start.AddHours(1);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
                {
                    CoachId = BobCoachId,
                    RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
                }, logBody: true));

            // The use case should reject past-dated availability slots
            await AssertHelper.AssertNotNull(exception.Message, "Exception message is returned for past-date slot");
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
        public async Task Handle_CreateAvailabilitySlot_GuidEmptyCoachId_ThrowsException()
        {
            // Arrange – Guid.Empty passes model validation but no coach exists in the DB for it
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(55).Date.AddHours(11));
            var end = start.AddHours(1);

            // Act & Assert – business logic should throw when coach cannot be found
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
                {
                    CoachId = Guid.Empty,
                    RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
                }, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "Exception is raised for Guid.Empty coach ID");
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
