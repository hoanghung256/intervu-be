using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
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

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.GetAsync("/api/v1/interview-room/upcoming-sessions", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request successful");
            await AssertHelper.AssertNotNull(payload.Data, "Upcoming sessions data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync("/api/v1/interview-room/upcoming-sessions", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_EmptyResults_ReturnsOk()
        {
            // Assuming Bob has no upcoming sessions
            var token = await LoginUserAsync("bob@example.com");

            var response = await _api.GetAsync("/api/v1/interview-room/upcoming-sessions", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for no upcoming sessions");
            await AssertHelper.AssertTrue(payload.Success, "Request successful");
            await AssertHelper.AssertEqual(0, payload.Data.GetArrayLength(), "Data should be an empty list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetUpcomingSessions_Pagination_ReturnsCorrectData()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.GetAsync("/api/v1/interview-room/upcoming-sessions?page=1&pageSize=5", jwtToken: token, logBody: true);
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

            var response = await _api.GetAsync("/api/v1/interview-room/upcoming-sessions?page=0", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid page 0 should return 400 Bad Request");
        }
    }
}
