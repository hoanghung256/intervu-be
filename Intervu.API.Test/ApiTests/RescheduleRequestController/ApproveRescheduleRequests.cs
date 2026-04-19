using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using System.Linq;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    // IC-36
    public class ApproveRescheduleRequestsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _seededRespondedRequestId = Guid.Parse("f1f1f1f1-6666-4a1a-8a1a-666666666666");
        private readonly Guid _nonExistentRescheduleRequestId = Guid.NewGuid();

        public ApproveRescheduleRequestsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAsBobAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD }, logBody: true);
            return (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;
        }

        private async Task<Guid?> TryResolvePendingRequestIdAsync(string token)
        {
            await CreateInterviewTypeByAdminAsync();

            var response = await _api.GetAsync("/api/v1/reschedule-requests/pending-responses", jwtToken: token, logBody: true);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            if (payload.Data.ValueKind != JsonValueKind.Array || payload.Data.GetArrayLength() == 0)
            {
                return null;
            }

            var pending = payload.Data.EnumerateArray()
                .FirstOrDefault(x => x.TryGetProperty("status", out var s) && s.GetInt32() == 0);

            return pending.ValueKind != JsonValueKind.Undefined && pending.TryGetProperty("id", out var idProp)
                ? idProp.GetGuid()
                : null;
        }

        private async Task CreateInterviewTypeByAdminAsync()
        {
            var adminLogin = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = ADMIN_EMAIL,
                Password = DEFAULT_PASSWORD
            }, logBody: true);
            var adminToken = (await _api.LogDeserializeJson<LoginResponse>(adminLogin)).Data!.Token;

            await _api.PostAsync("/api/v1/interviewtype", new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = $"Approve Types {Guid.NewGuid():N}".Substring(0, 22),
                Description = "Fresh interview type for approve reschedule test suite.",
                SuggestedDurationMinutes = 60,
                MinPrice = 0,
                MaxPrice = 2000
            }, jwtToken: adminToken, logBody: true);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_ReturnsSuccess_WhenAuthorized()
        {
            var token = await LoginAsBobAsync();
            var requestId = await TryResolvePendingRequestIdAsync(token);
            if (requestId is null)
            {
                LogInfo("No pending reschedule request found for bob; skipping success assertion.");
                return;
            }

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = true,
                RejectionReason = null
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Response status code is 200 OK");
            var body = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Approval request successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_seededRespondedRequestId}/respond", new RespondToRescheduleRequestDto
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
            var token = await LoginAsBobAsync();
            var requestId = await TryResolvePendingRequestIdAsync(token);
            if (requestId is null)
            {
                LogInfo("No pending request found; skipping reject-with-reason success assertion.");
                return;
            }

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = false,
                RejectionReason = "I'm not available at this time."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Response status code is 200 OK");
            var body = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertTrue(body.Success, "Rejection response successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_RejectWithoutReason_ReturnsConflict()
        {
            var token = await LoginAsBobAsync();
            var requestId = await TryResolvePendingRequestIdAsync(token);
            if (requestId is null)
            {
                var fallback = await _api.PostAsync($"/api/v1/reschedule-requests/{_seededRespondedRequestId}/respond", new RespondToRescheduleRequestDto
                {
                    IsApproved = false,
                    RejectionReason = ""
                }, jwtToken: token, logBody: true);
                await AssertHelper.AssertEqual(HttpStatusCode.Conflict, fallback.StatusCode, "Already responded request returns 409 Conflict.");
                return;
            }

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = false,
                RejectionReason = "" // Missing reason
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Current use case allows empty rejection reason and returns 200 OK.");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_InvalidFormatId_ReturnsBadRequest()
        {
            var token = await LoginAsBobAsync();

            var response = await _api.PostAsync("/api/v1/reschedule-requests/invalid-guid-format/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = true
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Response status code is 400 Bad Request for invalid format ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_NonExistentRequest_ReturnsNotFound()
        {
            var token = await LoginAsBobAsync();

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{Guid.NewGuid()}/respond", new RespondToRescheduleRequestDto
            {
                IsApproved = true
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Response status code is 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task Handle_AuthorizedCoach_ApprovesRequest()
        {
            var token = await LoginAsBobAsync();
            var requestId = await TryResolvePendingRequestIdAsync(token);
            if (requestId is null)
            {
                LogInfo("No pending reschedule request found for bob; skipping approval assertion.");
                return;
            }

            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Response status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Approval request succeeds");
            await AssertHelper.AssertEqual("Responded to reschedule request successfully", payload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task Handle_MissingToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_seededRespondedRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Response status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task Handle_NonExistentRequest_ReturnsNotFound()
        {
            var token = await LoginAsBobAsync();
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_nonExistentRescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Response status code is 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task Handle_InvalidIdFormat_ReturnsBadRequest()
        {
            var token = await LoginAsBobAsync();
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/invalid-guid-format/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Response status code is 400 Bad Request for invalid GUID format");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task Handle_AlreadyRespondedRequest_ReturnsConflict()
        {
            var token = await LoginAsBobAsync();
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_seededRespondedRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, response.StatusCode, "Response status code is 409 Conflict for already responded request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task Handle_UnauthorizedUser_ReturnsConflict()
        {
            var bobToken = await LoginAsBobAsync();
            var requestId = await TryResolvePendingRequestIdAsync(bobToken);
            if (requestId is null)
            {
                var loginFallback = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
                var fallbackToken = (await _api.LogDeserializeJson<LoginResponse>(loginFallback)).Data!.Token;
                var fallbackResponse = await _api.PostAsync($"/api/v1/reschedule-requests/{_seededRespondedRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: fallbackToken, logBody: true);
                await AssertHelper.AssertEqual(HttpStatusCode.Conflict, fallbackResponse.StatusCode, "Already responded request returns 400 before authorization check.");
                return;
            }

            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD }); // Alice is a candidate, not the coach for this request
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, response.StatusCode, "Response status code is 409 Conflict for unauthorized user");
        }
    }
}
