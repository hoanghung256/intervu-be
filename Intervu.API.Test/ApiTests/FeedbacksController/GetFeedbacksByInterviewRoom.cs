using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.FeedbacksController
{
    public class GetFeedbacksByInterviewRoomTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        // room1Id: seeded room that contains feedback 9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10
        private readonly Guid _seededRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");

        public GetFeedbacksByInterviewRoomTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        // ── Normal (Happy Path) ─────────────────────────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetFeedbacksByInterviewRoom_ReturnsSuccess_WhenCalledByCandidate()
        {
            // Arrange – alice is a Candidate; CandidateOrInterviewer policy allows her
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // Act
            var response = await _api.GetAsync($"/api/v1/feedbacks/interview-room/{_seededRoomId}", jwtToken: loginData.Data!.Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for Candidate role");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Response reports success = true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetFeedbacksByInterviewRoom_ReturnsSuccess_WhenCalledByCoach()
        {
            // Arrange – bob is a Coach (Interviewer); CandidateOrInterviewer policy allows him
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // Act
            var response = await _api.GetAsync($"/api/v1/feedbacks/interview-room/{_seededRoomId}", jwtToken: loginData.Data!.Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for Coach role");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Response reports success = true");
        }

        // ── Boundary (Edge Cases) ───────────────────────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetFeedbacksByInterviewRoom_ReturnsEmptyData_WhenRoomDoesNotExist()
        {
            // Arrange – random Guid guaranteed not to exist in the seeded database
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var nonExistentRoomId = Guid.NewGuid();

            // Act
            var response = await _api.GetAsync($"/api/v1/feedbacks/interview-room/{nonExistentRoomId}", jwtToken: loginData.Data!.Token, logBody: true);

            // Assert – repository returns empty collection; controller wraps it in 200 OK
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK even when room has no feedbacks");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Response reports success = true with empty data");
        }

        // ── Abnormal (Auth Failures) ────────────────────────────────────────────

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetFeedbacksByInterviewRoom_ReturnsUnauthorized_WhenNoToken()
        {
            // Act – no Authorization header
            var response = await _api.GetAsync($"/api/v1/feedbacks/interview-room/{_seededRoomId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetFeedbacksByInterviewRoom_ReturnsForbidden_WhenCalledByAdmin()
        {
            // Arrange – Admin role is outside the CandidateOrInterviewer policy
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // Act
            var response = await _api.GetAsync($"/api/v1/feedbacks/interview-room/{_seededRoomId}", jwtToken: loginData.Data!.Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden for Admin role");
        }
    }
}
