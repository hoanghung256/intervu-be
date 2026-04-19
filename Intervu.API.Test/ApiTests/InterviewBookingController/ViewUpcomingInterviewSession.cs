using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    // IC-40
    public class ViewUpcomingInterviewSessionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewUpcomingInterviewSessionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginUserAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<string> RegisterAndLoginNewCoachAsync()
        {
            var email = $"coach_empty_upcoming_{Guid.NewGuid():N}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                FullName = "Coach Empty Upcoming",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = "Coach",
                SlugProfileUrl = $"coach-empty-upcoming-{Guid.NewGuid():N}"
            }, logBody: true);

            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD
            }, logBody: true);
            return (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=0", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request successful");
            await AssertHelper.AssertNotNull(payload.Data, "Upcoming sessions data is returned");

            var items = payload.Data.GetProperty("items");
            foreach (var item in items.EnumerateArray())
            {
                if (item.TryGetProperty("status", out var statusElement))
                {
                    if (statusElement.ValueKind == JsonValueKind.Number)
                    {
                        await AssertHelper.AssertEqual((int)InterviewRoomStatus.Scheduled, statusElement.GetInt32(), "Only scheduled rooms are returned");
                    }
                    else if (statusElement.ValueKind == JsonValueKind.String)
                    {
                        await AssertHelper.AssertEqual(nameof(InterviewRoomStatus.Scheduled), statusElement.GetString(), "Only scheduled rooms are returned");
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=0", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_EmptyResults_ReturnsOk()
        {
            var token = await RegisterAndLoginNewCoachAsync();

            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=0", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for no upcoming sessions");
            await AssertHelper.AssertTrue(payload.Success, "Request successful");
            await AssertHelper.AssertEqual(0, payload.Data.GetProperty("items").GetArrayLength(), "Data items should be an empty list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_Pagination_ReturnsCorrectData()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=0&page=1&pageSize=5", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for paginated request");
            await AssertHelper.AssertTrue(payload.Success, "Paginated request successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_InvalidPage_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=0&page=0", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid page 0 should return 400 Bad Request");
        }
    }
}
