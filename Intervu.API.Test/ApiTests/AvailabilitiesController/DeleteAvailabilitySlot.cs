using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AvailabilitiesController
{
    public class DeleteAvailabilitySlotTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly HttpClient _client;
        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        public DeleteAvailabilitySlotTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _client = factory.CreateClient();
            _api = new ApiHelper(_client);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task DeleteAvailabilitySlot_ReturnsSuccess()
        {
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(67).Date.AddHours(3));
            var end = start.AddHours(1);
            await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            var deleteResponse = await DeleteWithBodyAsync("/api/v1/availabilities/range", new CoachAvailabilityDeleteDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            var deletePayload = await _api.LogDeserializeJson<JsonElement>(deleteResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete range status is 200 OK");
            await AssertHelper.AssertTrue(deletePayload.Success, "Delete range succeeds");
            await AssertHelper.AssertEqual("Range deleted", deletePayload.Message, "Delete message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task DeleteAvailabilitySlotById_ReturnsSuccess()
        {
            // Arrange – create a slot and retrieve its ID from the response
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(80).Date.AddHours(5));
            var end = start.AddHours(1);
            var createResponse = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);
            var slotId = createPayload.Data!.GetProperty("ids").EnumerateArray().First().GetGuid();

            // Act
            var deleteResponse = await _api.DeleteAsync($"/api/v1/availabilities/{slotId}", logBody: true);

            // Assert
            var deletePayload = await _api.LogDeserializeJson<JsonElement>(deleteResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete by ID status is 200 OK");
            await AssertHelper.AssertTrue(deletePayload.Success, "Delete by ID succeeds");
            await AssertHelper.AssertEqual("Deleted", deletePayload.Message, "Delete by ID message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_DeleteAvailabilitySlotById_NonExistentId_ThrowsException()
        {
            // Arrange – a random GUID that does not exist in the database
            var nonExistentId = Guid.NewGuid();

            // Act & Assert – the use case should throw when the slot is not found
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _api.DeleteAsync($"/api/v1/availabilities/{nonExistentId}", logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "Exception is raised for non-existent availability ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_DeleteAvailabilitySlotById_GuidEmpty_ThrowsException()
        {
            // Arrange – Guid.Empty passes route parsing but no slot exists for it
            var emptySlotId = Guid.Empty;

            // Act & Assert – use case should throw when the slot cannot be found
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _api.DeleteAsync($"/api/v1/availabilities/{emptySlotId}", logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "Exception is raised when deleting Guid.Empty slot ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task Handle_DeleteAvailabilityRange_NoSlotsInRange_ReturnsSuccess()
        {
            // Arrange – a future time range that has no slots created (idempotent delete)
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(95).Date.AddHours(2));
            var end = start.AddHours(1);

            // Act – delete a range with no existing slots
            var deleteResponse = await DeleteWithBodyAsync("/api/v1/availabilities/range", new CoachAvailabilityDeleteDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            });

            // Assert – idempotent: should succeed even when no slots exist in the range
            var deletePayload = await _api.LogDeserializeJson<JsonElement>(deleteResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Empty-range delete returns 200 OK");
            await AssertHelper.AssertTrue(deletePayload.Success, "Empty-range delete succeeds");
            await AssertHelper.AssertEqual("Range deleted", deletePayload.Message, "Delete message matches for empty range");
        }

        private async Task<HttpResponseMessage> DeleteWithBodyAsync(string requestUri, object payload, string jwtToken = "")
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            if (!string.IsNullOrWhiteSpace(jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            return await _client.SendAsync(request);
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
