using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    public class ApproveRescheduleRequestsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _rescheduleRequestId = Guid.Parse("f1f1f1f1-6666-4a1a-8a1a-666666666666");

        public ApproveRescheduleRequestsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_ReturnsSuccess_WhenAuthorized()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = true,
                RejectionReason = null
            }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Response status code is 200 OK");
            var body = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Approval request successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = true,
                RejectionReason = null
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Response status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_RejectWithReason_ReturnsSuccess()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = false,
                RejectionReason = "I'm not available at this time."
            }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Response status code is 200 OK");
            var body = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Rejection response successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_RejectWithoutReason_ReturnsBadRequest()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = false,
                RejectionReason = "" // Missing reason
            }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Response status code is 400 Bad Request for missing reason on rejection");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_InvalidFormatId_ReturnsBadRequest()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PostAsync("/api/v1/reschedule-requests/invalid-guid-format/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = true
            }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Response status code is 400 Bad Request for invalid format ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_NonExistentRequest_ReturnsNotFound()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{Guid.NewGuid()}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = true
            }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Response status code is 404 Not Found");
        }
    }
}
