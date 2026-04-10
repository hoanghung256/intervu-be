using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    // IC-52
    public class ReportInterviewProblemTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _existingRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"); // Assuming this is a valid room ID for testing

        public ReportInterviewProblemTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task ReportInterviewProblem_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("alice@example.com"); // Assuming Alice is in the room

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/report", new CreateRoomReportRequest
            {
                Reason = "Audio issue",
                Details = "Mic was disconnected during interview"
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Report problem status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Report problem successful");
            await AssertHelper.AssertEqual("Problem reported successfully", payload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ReportInterviewProblem_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/report", new CreateRoomReportRequest
            {
                Reason = "Video issue",
                Details = "Camera not working"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ReportInterviewProblem_NonExistentRoom_ReturnsNotFound()
        {
            var token = await LoginUserAsync("alice@example.com");
            var nonExistentRoomId = Guid.NewGuid();

            var response = await _api.PostAsync($"/api/v1/interview-room/{nonExistentRoomId}/report", new CreateRoomReportRequest
            {
                Reason = "Room not found",
                Details = "Attempted to report problem for a room that does not exist."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent room ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ReportInterviewProblem_MissingReason_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/report", new CreateRoomReportRequest
            {
                Reason = "", // Missing reason
                Details = "Details provided but reason is empty."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing reason should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ReportInterviewProblem_UserNotInRoom_ReturnsForbidden()
        {
            var token = await LoginUserAsync("bob@example.com"); // Assuming Bob is NOT in _existingRoomId

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/report", new CreateRoomReportRequest
            {
                Reason = "Unauthorized report",
                Details = "User not part of this room."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in room should get 403 Forbidden");
        }
    }
}
