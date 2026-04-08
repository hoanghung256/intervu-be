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
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(12));
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
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
