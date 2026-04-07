using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Availability
{
    public class AvailabilitiesControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly HttpClient _client;

        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        public AvailabilitiesControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _client = factory.CreateClient();
            _api = new ApiHelper(_client);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public async Task AvailabilityManagement_CreateUpdateDeleteRange_ReturnsSuccess()
        {
            // Arrange
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(12));
            var end = start.AddHours(2);

            var createRequest = new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            };

            // Act - Create
            LogInfo("Creating coach availability blocks.");
            var createResponse = await _api.PostAsync("/api/v1/availabilities", createRequest, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);

            // Assert - Create
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status is 200 OK");
            await AssertHelper.AssertTrue(createPayload.Success, "Create availability succeeds");
            var ids = ExtractGuidList(createPayload.Data!.GetProperty("ids"));
            await AssertHelper.AssertTrue(ids.Count >= 4, "Create returns split 30-minute blocks");

            // Act - Update
            var newStart = start.AddMinutes(30);
            var newEnd = end.AddMinutes(30);
            var updateRequest = new CoachAvailabilityUpdateDto
            {
                CoachId = BobCoachId,
                OriginalStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                OriginalEndTime = new DateTimeOffset(end, TimeSpan.Zero),
                NewStartTime = new DateTimeOffset(newStart, TimeSpan.Zero),
                NewEndTime = new DateTimeOffset(newEnd, TimeSpan.Zero)
            };

            LogInfo("Updating coach availability range.");
            var updateResponse = await _api.PutAsync("/api/v1/availabilities", updateRequest, logBody: true);
            var updatePayload = await _api.LogDeserializeJson<JsonElement>(updateResponse, logBody: true);

            // Assert - Update
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status is 200 OK");
            await AssertHelper.AssertTrue(updatePayload.Success, "Update availability succeeds");

            // Act - Delete range
            var deleteRangeRequest = new CoachAvailabilityDeleteDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(newStart, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(newEnd, TimeSpan.Zero)
            };

            LogInfo("Deleting updated availability range.");
            var deleteResponse = await DeleteWithBodyAsync("/api/v1/availabilities/range", deleteRangeRequest);
            var deletePayload = await _api.LogDeserializeJson<JsonElement>(deleteResponse, logBody: true);

            // Assert - Delete range
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete range status is 200 OK");
            await AssertHelper.AssertTrue(deletePayload.Success, "Delete range succeeds");
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

        private static List<Guid> ExtractGuidList(JsonElement idsElement)
        {
            var ids = new List<Guid>();
            foreach (var id in idsElement.EnumerateArray())
            {
                ids.Add(id.GetGuid());
            }

            return ids;
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
