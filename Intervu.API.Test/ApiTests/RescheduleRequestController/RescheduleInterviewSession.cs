using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    // IC-34
    public class RescheduleInterviewSessionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _roomRescheduleCreateId = Guid.Parse("b1b1b1b1-2222-4a1a-8a1a-222222222222"); // Assuming this is a valid, upcoming room ID
        private readonly Guid _nonExistentRoomId = Guid.NewGuid();

        public RescheduleInterviewSessionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAsAliceAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        private async Task<string> LoginAsBobAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_WithValidData_ReturnsSuccess()
        {
            var token = await LoginAsAliceAsync();
            var newStartTime = DateTime.UtcNow.AddDays(2).AddHours(1).AddMinutes(30); // Ensure it's in the future and aligned
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                NewStartTime = newStartTime,
                Reason = "Need to reschedule due to personal emergency that requires my immediate attention."
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Reschedule request successful");
            await AssertHelper.AssertEqual("Reschedule request created successfully", payload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                NewStartTime = DateTime.UtcNow.AddDays(2),
                Reason = "Unauthorized attempt."
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_NonExistentRoom_ReturnsNotFound()
        {
            var token = await LoginAsAliceAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _nonExistentRoomId,
                NewStartTime = DateTime.UtcNow.AddDays(2),
                Reason = "Room does not exist."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent room ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_NewStartTimeInPast_ReturnsBadRequest()
        {
            var token = await LoginAsAliceAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                NewStartTime = DateTime.UtcNow.AddDays(-1), // Past date
                Reason = "Cannot reschedule to a past time."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Past NewStartTime should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_MissingReason_ReturnsBadRequest()
        {
            var token = await LoginAsAliceAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                NewStartTime = DateTime.UtcNow.AddDays(3),
                Reason = "" // Empty reason
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing reason should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_UserNotInRoom_ReturnsForbidden()
        {
            var token = await LoginAsBobAsync(); // Assuming Bob is not in _roomRescheduleCreateId
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                NewStartTime = DateTime.UtcNow.AddDays(4),
                Reason = "User not part of this interview session."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in room should get 403 Forbidden");
        }
    }
}
