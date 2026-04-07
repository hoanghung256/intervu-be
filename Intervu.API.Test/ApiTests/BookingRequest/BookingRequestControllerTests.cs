using System.Net;
using System.Text.Json;
using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Availability;
using Intervu.Application.DTOs.BookingRequest;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.BookingRequest
{
    public class BookingRequestControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        private static readonly Guid BobCoachId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
        private const string AliceEmail = "alice@example.com";

        public BookingRequestControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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

            var allBlocks = await CreateAvailabilityBlocksAsync(requiredBlocks, dayOffset: 20, hourOffset: 9);
            var round1Ids = allBlocks.Take(requiredBlocks).ToList();

            var createDto = new CreateJDBookingRequestDto
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
                        AvailabilityIds = round1Ids
                    }
                ]
            };

            LogInfo("Creating single-round JD booking request.");
            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", createDto, jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Single round booking status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Single round booking succeeds");

            var rounds = payload.Data!.GetProperty("rounds");
            await AssertHelper.AssertEqual(1, rounds.GetArrayLength(), "Single round booking returns exactly one round");
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
            var totalRequired = required1 + required2;

            var allBlocks = await CreateAvailabilityBlocksAsync(totalRequired, dayOffset: 21, hourOffset: 11);
            var round1Ids = allBlocks.Take(required1).ToList();
            var round2Ids = allBlocks.Skip(required1).Take(required2).ToList();

            var createDto = new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = "https://example.com/jd-multi.pdf",
                CVUrl = "https://example.com/cv-multi.pdf",
                AimLevel = AimLevel.Senior,
                Rounds =
                [
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service1.Id,
                        AvailabilityIds = round1Ids
                    },
                    new CreateInterviewRoundDto
                    {
                        CoachInterviewServiceId = service2.Id,
                        AvailabilityIds = round2Ids
                    }
                ]
            };

            LogInfo("Creating multi-round JD booking request.");
            var response = await _api.PostAsync("/api/v1/booking-requests/jd-interview", createDto, jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Multi-round booking status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Multi-round booking succeeds");

            var rounds = payload.Data!.GetProperty("rounds");
            await AssertHelper.AssertEqual(2, rounds.GetArrayLength(), "Multi-round booking returns two rounds");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_SingleRound_ReturnsCancelledStatus()
        {
            var token = await LoginSeededCandidateAsync();
            var bookingId = await CreateBookingAndGetIdAsync(token, isMultipleRounds: false, dayOffset: 22, hourOffset: 14);

            LogInfo("Cancelling single-round booking request.");
            var cancelResponse = await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: token, logBody: true);
            var cancelPayload = await _api.LogDeserializeJson<JsonElement>(cancelResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, cancelResponse.StatusCode, "Single-round cancel status is 200 OK");
            await AssertHelper.AssertTrue(cancelPayload.Success, "Single-round cancellation succeeds");
            await AssertHelper.AssertEqual((int)BookingRequestStatus.Cancelled, cancelPayload.Data!.GetProperty("status").GetInt32(), "Single-round booking status is Cancelled");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task CancelBookingRequest_MultipleRounds_ReturnsCancelledStatus()
        {
            var token = await LoginSeededCandidateAsync();
            var bookingId = await CreateBookingAndGetIdAsync(token, isMultipleRounds: true, dayOffset: 23, hourOffset: 16);

            LogInfo("Cancelling multi-round booking request.");
            var cancelResponse = await _api.PostAsync<object>($"/api/v1/booking-requests/{bookingId}/cancel", null, jwtToken: token, logBody: true);
            var cancelPayload = await _api.LogDeserializeJson<JsonElement>(cancelResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, cancelResponse.StatusCode, "Multi-round cancel status is 200 OK");
            await AssertHelper.AssertTrue(cancelPayload.Success, "Multi-round cancellation succeeds");
            await AssertHelper.AssertEqual((int)BookingRequestStatus.Cancelled, cancelPayload.Data!.GetProperty("status").GetInt32(), "Multi-round booking status is Cancelled");
        }

        private async Task<Guid> CreateBookingAndGetIdAsync(string candidateToken, bool isMultipleRounds, int dayOffset, int hourOffset)
        {
            var services = await GetCoachServicesAsync();
            var service1 = services[0];
            var service2 = services.Count > 1 ? services[1] : services[0];

            var required1 = GetRequiredBlockCount(service1.DurationMinutes);
            var required2 = isMultipleRounds ? GetRequiredBlockCount(service2.DurationMinutes) : 0;
            var totalRequired = required1 + required2;

            var allBlocks = await CreateAvailabilityBlocksAsync(totalRequired, dayOffset, hourOffset);
            var rounds = new List<CreateInterviewRoundDto>
            {
                new()
                {
                    CoachInterviewServiceId = service1.Id,
                    AvailabilityIds = allBlocks.Take(required1).ToList()
                }
            };

            if (isMultipleRounds)
            {
                rounds.Add(new CreateInterviewRoundDto
                {
                    CoachInterviewServiceId = service2.Id,
                    AvailabilityIds = allBlocks.Skip(required1).Take(required2).ToList()
                });
            }

            var request = new CreateJDBookingRequestDto
            {
                CoachId = BobCoachId,
                JobDescriptionUrl = isMultipleRounds ? "https://example.com/jd-cancel-multi.pdf" : "https://example.com/jd-cancel-single.pdf",
                CVUrl = isMultipleRounds ? "https://example.com/cv-cancel-multi.pdf" : "https://example.com/cv-cancel-single.pdf",
                AimLevel = AimLevel.Senior,
                Rounds = rounds
            };

            var createResponse = await _api.PostAsync("/api/v1/booking-requests/jd-interview", request, jwtToken: candidateToken, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);

            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            Assert.True(createPayload.Success);
            return createPayload.Data!.GetProperty("id").GetGuid();
        }

        private async Task<string> LoginSeededCandidateAsync()
        {
            var loginRequest = new LoginRequest
            {
                Email = AliceEmail,
                Password = ACCOUNT_PASSWORD
            };

            var response = await _api.PostAsync("/api/v1/account/login", loginRequest, logBody: true);
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

            var createRequest = new CoachAvailabilityCreateDto
            {
                CoachId = BobCoachId,
                RangeStartTime = new DateTimeOffset(start, TimeSpan.Zero),
                RangeEndTime = new DateTimeOffset(end, TimeSpan.Zero)
            };

            var createResponse = await _api.PostAsync("/api/v1/availabilities", createRequest, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);

            Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
            Assert.True(createPayload.Success);

            var ids = new List<Guid>();
            foreach (var id in createPayload.Data!.GetProperty("ids").EnumerateArray())
            {
                ids.Add(id.GetGuid());
            }

            return ids;
        }

        private static int GetRequiredBlockCount(int durationMinutes)
        {
            return (durationMinutes + 29) / 30;
        }

        private static DateTime AlignToHalfHourUtc(DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            var roundedMinute = utc.Minute < 30 ? 0 : 30;
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, roundedMinute, 0, DateTimeKind.Utc);
        }
    }
}
