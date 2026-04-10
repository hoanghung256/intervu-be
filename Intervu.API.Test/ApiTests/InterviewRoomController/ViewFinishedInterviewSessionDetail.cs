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
        private readonly Guid _existingSessionId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Assuming this is a valid finished session ID for testing

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
        public async Task GetFinishedSessionDetail_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("alice@example.com"); // Assuming Alice participated in _existingSessionId

            var response = await _api.GetAsync($"/api/v1/interview-room/finished-sessions/{_existingSessionId}", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Request successful");
            await AssertHelper.AssertNotNull(payload.Data, "Finished session detail data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetFinishedSessionDetail_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync($"/api/v1/interview-room/finished-sessions/{_existingSessionId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetFinishedSessionDetail_NonExistentSession_ReturnsNotFound()
        {
            var token = await LoginUserAsync("alice@example.com");
            var nonExistentSessionId = Guid.NewGuid();

            var response = await _api.GetAsync($"/api/v1/interview-room/finished-sessions/{nonExistentSessionId}", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent session ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetFinishedSessionDetail_UserNotInSession_ReturnsForbidden()
        {
            var token = await LoginUserAsync("bob@example.com"); // Assuming Bob did NOT participate in _existingSessionId

            var response = await _api.GetAsync($"/api/v1/interview-room/finished-sessions/{_existingSessionId}", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in session should get 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetFinishedSessionDetail_InvalidSessionIdFormat_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("alice@example.com");
            var invalidSessionId = "not-a-valid-guid";

            var response = await _api.GetAsync($"/api/v1/interview-room/finished-sessions/{invalidSessionId}", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid session ID format should return 400 Bad Request");
        }
    }
}
