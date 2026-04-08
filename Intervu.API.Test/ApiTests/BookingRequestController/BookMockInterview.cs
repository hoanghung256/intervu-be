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
    public class BookMockInterviewTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");

        public BookMockInterviewTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CreateJDBookingRequest_SingleRound_ReturnsSuccess()
        {
            var token = await LoginSeededCandidateAsync();
            var services = await GetCoachServicesAsync();
            var service1 = services.First();
            var requiredBlocks = GetRequiredBlockCount(service1.DurationMinutes);
            var allBlocks = await CreateAvailabilityBlocksAsync(requiredBlocks, 20, 9);

            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/jd-single.pdf",
                CVUrl = "https://example.com/cv-single.pdf",
                AimLevel = AimLevel.MidLevel,
                Rounds =
                [
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service1.Id,
                        AvailabilityIds = allBlocks.Take(requiredBlocks).ToList()
                    }
                ]
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Single round booking status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Single round booking succeeds");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CreateJDBookingRequest_MultipleRounds_ReturnsSuccess()
        {
            var token = await LoginSeededCandidateAsync();
            var services = await GetCoachServicesAsync();
            var service1 = services[0];
            var service2 = services.Count > 1 ? services[1] : services[0];
            var required1 = GetRequiredBlockCount(service1.DurationMinutes);
            var required2 = GetRequiredBlockCount(service2.DurationMinutes);
            var allBlocks = await CreateAvailabilityBlocksAsync(required1 + required2, 21, 11);

            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/jd-multi.pdf",
                CVUrl = "https://example.com/cv-multi.pdf",
                AimLevel = AimLevel.Senior,
                Rounds =
                [
                    new CreateInterviewRoundDto { CoachInterviewServiceId = service1.Id, AvailabilityIds = allBlocks.Take(required1).ToList() },
                    new CreateInterviewRoundDto { CoachInterviewServiceId = service2.Id, AvailabilityIds = allBlocks.Skip(required1).Take(required2).ToList() }
                ]
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Multi-round booking status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Multi-round booking succeeds");
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
            var start = AlignToHalfHourUtc(DateTime.UtcNow.AddDays(dayOffset).Date.AddHours(hourOffset));
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

        private static int GetRequiredBlockCount(int durationMinutes) => (durationMinutes + 29) / 30;

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
