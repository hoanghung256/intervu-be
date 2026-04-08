using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.BookingRequestController
{
    public class CancelInterviewSessionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        public CancelInterviewSessionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_SingleRound_ReturnsCancelledStatus()
        {
            var token = await LoginSeededCandidateAsync();
            var bookingId = await CreateBookingAndGetIdAsync(token, false, 22, 14);

            var cancelResponse = await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: token, logBody: true);
            var cancelPayload = await _api.LogDeserializeJson<JsonElement>(cancelResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, cancelResponse.StatusCode, "Single-round cancel status is 200 OK");
            await AssertHelper.AssertEqual((int)BookingRequestStatus.Cancelled, cancelPayload.Data!.GetProperty("status").GetInt32(), "Single-round booking status is Cancelled");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_MultipleRounds_ReturnsCancelledStatus()
        {
            var token = await LoginSeededCandidateAsync();
            var bookingId = await CreateBookingAndGetIdAsync(token, true, 23, 16);

            var cancelResponse = await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: token, logBody: true);
            var cancelPayload = await _api.LogDeserializeJson<JsonElement>(cancelResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, cancelResponse.StatusCode, "Multi-round cancel status is 200 OK");
            await AssertHelper.AssertEqual((int)BookingRequestStatus.Cancelled, cancelPayload.Data!.GetProperty("status").GetInt32(), "Multi-round booking status is Cancelled");
        }

        private async Task<Guid> CreateBookingAndGetIdAsync(string candidateToken, bool isMultipleRounds, int dayOffset, int hourOffset)
        {
            var services = await GetCoachServicesAsync();
            var service1 = services[0];
            var service2 = services.Count > 1 ? services[1] : services[0];
            var required1 = (service1.DurationMinutes + 29) / 30;
            var required2 = isMultipleRounds ? (service2.DurationMinutes + 29) / 30 : 0;
            var allBlocks = await CreateAvailabilityBlocksAsync(required1 + required2, dayOffset, hourOffset);

            var rounds = new List<CreateInterviewRoundDto>
            {
                new() { CoachInterviewServiceId = service1.Id, AvailabilityIds = allBlocks.Take(required1).ToList() }
            };
            if (isMultipleRounds)
            {
                rounds.Add(new CreateInterviewRoundDto { CoachInterviewServiceId = service2.Id, AvailabilityIds = allBlocks.Skip(required1).Take(required2).ToList() });
            }

            var createResponse = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/jd.pdf",
                CVUrl = "https://example.com/cv.pdf",
                AimLevel = AimLevel.Senior,
                Rounds = rounds
            }, jwtToken: candidateToken, logBody: true);

            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);
            return createPayload.Data!.GetProperty("id").GetGuid();
        }

        private async Task<string> LoginSeededCandidateAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD }, logBody: true);
            var payload = await _api.LogDeserializeJson<LoginResponse>(response, logBody: true);
            return payload.Data!.Token;
        }

        private async Task<List<CoachInterviewServiceDto>> GetCoachServicesAsync()
        {
            var response = await _api.GetAsync($"/api/v1/coach-interview-services/coach/{BobCoachId}", logBody: true);
            var payload = await _api.LogDeserializeJson<List<CoachInterviewServiceDto>>(response, logBody: true);
            return payload.Data!;
        }

        private async Task<List<Guid>> CreateAvailabilityBlocksAsync(int requiredBlocks, int dayOffset, int hourOffset)
        {
            var start = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(dayOffset).Date.AddHours(hourOffset), DateTimeKind.Utc);
            var end = start.AddMinutes(requiredBlocks * 30);
            var createResponse = await _api.PostAsync("/api/v1/availabilities", new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            }, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);
            return createPayload.Data!.GetProperty("ids").EnumerateArray().Select(x => x.GetGuid()).ToList();
        }
    }
}
