using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AvailabilitiesController
{
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
        public async Task Handle_UpdateAvailabilitySlot_NewEndBeforeNewStart_ThrowsArgumentException()
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

            // Act & Assert – the new range is inverted (NewEndTime < NewStartTime)
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
                {
                    CoachId = BobCoachId,
                    OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                    NewStartTime = new DateTimeOffset(end, TimeSpan.Zero),
                    NewEndTime = new DateTimeOffset(start, TimeSpan.Zero)
                }, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "ArgumentException is raised for inverted new time range");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_UpdateAvailabilitySlot_SameNewStartAndEnd_ThrowsArgumentException()
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

            // Act & Assert – zero-duration new range
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
                {
                    CoachId = BobCoachId,
                    OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                    NewStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    NewEndTime = new DateTimeOffset(start, TimeSpan.Zero)
                }, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "ArgumentException is raised for zero-duration new range");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_UpdateAvailabilitySlot_NonExistentOriginalSlot_ThrowsException()
        {
            // Arrange – a time range that was never created (no availability blocks exist for it)
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(102).Date.AddHours(8));
            var end = start.AddHours(1);

            // Act & Assert – use case should throw when original slots cannot be found
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
                {
                    CoachId = BobCoachId,
                    OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                    NewStartTime = new DateTimeOffset(start.AddHours(2), TimeSpan.Zero),
                    NewEndTime = new DateTimeOffset(end.AddHours(2), TimeSpan.Zero)
                }, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "Exception is raised when original slot range does not exist");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_UpdateAvailabilitySlot_GuidEmptyCoachId_ThrowsException()
        {
            // Arrange – Guid.Empty passes model validation but coach does not exist in the DB
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(108).Date.AddHours(9));
            var end = start.AddHours(1);

            // Act & Assert – use case should throw when coach cannot be found
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _api.PutAsync("/api/v1/availabilities", new CoachAvailabilityUpdateDto
                {
                    CoachId = Guid.Empty,
                    OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                    OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                    NewStartTime = new DateTimeOffset(start.AddHours(1), TimeSpan.Zero),
                    NewEndTime = new DateTimeOffset(end.AddHours(1), TimeSpan.Zero)
                }, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "Exception is raised for Guid.Empty coach ID on update");
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
