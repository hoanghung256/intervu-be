using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    // IC-48
    public class ViewFinishedInterviewSessionDetailTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewFinishedInterviewSessionDetailTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task GetFinishedSessions_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=2", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request successful");
            await AssertHelper.AssertNotNull(payload.Data, "Finished sessions data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetFinishedSessions_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=2", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetFinishedSessions_EmptyResults_ReturnsOk()
        {
            var token = await LoginUserAsync("bob@example.com");
            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=2", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for empty finished sessions");
            await AssertHelper.AssertTrue(payload.Success, "Request successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetFinishedSessions_InvalidPage_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("alice@example.com");
            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=2&page=0", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid page should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetFinishedSessions_InvalidStatusFormat_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("alice@example.com");
            var response = await _api.GetAsync("/api/v1/interviewroom?Statuses=invalid", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid status format should return 400 Bad Request");
        }
    }
}
