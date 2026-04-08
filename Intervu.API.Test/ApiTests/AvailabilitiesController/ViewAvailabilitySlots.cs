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

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
