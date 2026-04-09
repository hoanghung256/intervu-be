using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    // IC-36
    public class ApproveRescheduleRequestTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _rescheduleRequestId = Guid.Parse("f1f1f1f1-6666-4a1a-8a1a-666666666666"); // Assuming this ID exists and is pending
        private readonly Guid _nonExistentRescheduleRequestId = Guid.NewGuid();

        public ApproveRescheduleRequestTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        public async Task Handle_AuthorizedCoach_ApprovesRequest()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Response status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Approval request succeeds");
            await AssertHelper.AssertEqual("Reschedule request responded successfully", payload.Message, "Success message matches");
        }

        [Fact]
        public async Task Handle_MissingToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Response status code is 401 Unauthorized");
        }

        [Fact]
        public async Task Handle_NonExistentRequest_ReturnsNotFound()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_nonExistentRescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Response status code is 404 Not Found");
        }

        [Fact]
        public async Task Handle_InvalidIdFormat_ReturnsBadRequest()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/invalid-guid-format/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Response status code is 400 Bad Request for invalid GUID format");
        }

        [Fact]
        public async Task Handle_AlreadyRespondedRequest_ReturnsBadRequest()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;

            // First response (approve)
            await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);

            // Second response (try to approve again)
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Response status code is 400 Bad Request for already responded request");
        }

        [Fact]
        public async Task Handle_UnauthorizedUser_ReturnsForbidden()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD }); // Alice is a candidate, not the coach for this request
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Response status code is 403 Forbidden for unauthorized user");
        }
    }
}
