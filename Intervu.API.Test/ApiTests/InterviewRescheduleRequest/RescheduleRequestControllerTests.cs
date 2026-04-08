using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.DTOs.User;
using Xunit;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRescheduleRequest
{
    public class RescheduleRequestControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public RescheduleRequestControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> LoginSeededUserAsync(string email, string role = "Candidate")
        {
            var password = role == "Admin" || role == "Coach" || email.Contains("alice") ? DEFAULT_PASSWORD : CANDIDATE_PASSWORD;
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        // Seeded IDs from IntervuPostgreDbContext
        private readonly Guid _roomRescheduleCreateId = Guid.Parse("b1b1b1b1-2222-4a1a-8a1a-222222222222");
        private readonly Guid _roomRescheduleRespondId = Guid.Parse("c1c1c1c1-3333-4a1a-8a1a-333333333333");
        private readonly Guid _availProposedCreateId = Guid.Parse("d1d1d1d1-4444-4a1a-8a1a-444444444444");
        private readonly Guid _availProposedRespondId = Guid.Parse("e1e1e1e1-5555-4a1a-8a1a-555555555555");
        private readonly Guid _rescheduleRequestId = Guid.Parse("f1f1f1f1-6666-4a1a-8a1a-666666666666");

        private readonly string _aliceEmail = "alice@example.com";
        private readonly string _bobEmail = "bob@example.com";

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var (token, _) = await LoginSeededUserAsync(_aliceEmail, "Candidate");

            var dto = new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                ProposedAvailabilityId = _availProposedCreateId,
                Reason = "Need to reschedule due to personal emergency that requires my immediate attention"
            };

            // Act
            LogInfo($"Creating reschedule request for room {_roomRescheduleCreateId}");
            var response = await _api.PostAsync("/api/v1/reschedule-requests", dto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CreateRescheduleResponseData>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response should indicate success");
            await AssertHelper.AssertNotNull(apiResponse.Data?.RequestId, "RequestId should be returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task RespondToRescheduleRequest_ReturnsSuccess_WhenAuthorized()
        {
            // Arrange
            var (token, _) = await LoginSeededUserAsync(_bobEmail, "Coach");
            var requestId = _rescheduleRequestId; // Seeded request for roomRescheduleRespondId

            var respondDto = new RespondToRescheduleRequestDto
            {
                IsApproved = true,
                RejectionReason = null
            };

            // Act
            LogInfo($"Responding to reschedule request {requestId} as Bob.");
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", respondDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Response status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Response was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new CreateRescheduleRequestDto
            {
                RoomId = Guid.NewGuid(),
                ProposedAvailabilityId = Guid.NewGuid(),
                Reason = "Need to reschedule due to personal emergency"
            };

            // Act
            LogInfo("Creating reschedule request without authentication");
            var response = await _api.PostAsync("/api/v1/reschedule-requests", dto, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_WithShortReason_ReturnsBadRequest()
        {
            // Arrange
            var (token, _) = await LoginSeededUserAsync(_aliceEmail, "Candidate");
            var dto = new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                ProposedAvailabilityId = _availProposedCreateId,
                Reason = "Short" // Less than 10 characters
            };

            // Act
            LogInfo("Creating reschedule request with short reason.");
            var response = await _api.PostAsync("/api/v1/reschedule-requests", dto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task GetRescheduleRequestById_ReturnsSuccess_WhenIdIsValid()
        {
            // Arrange
            var (token, _) = await LoginSeededUserAsync(_aliceEmail, "Candidate");
            var requestId = _rescheduleRequestId;

            // Act
            LogInfo($"Getting reschedule request by ID: {requestId}");
            var response = await _api.GetAsync($"/api/v1/reschedule-requests/{requestId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response should indicate success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task GetMyRescheduleRequests_ReturnsSuccess_WhenAuthenticated()
        {
            // Arrange
            var (token, _) = await LoginSeededUserAsync(_aliceEmail, "Candidate");

            // Act
            LogInfo("Getting my reschedule requests.");
            var response = await _api.GetAsync("/api/v1/reschedule-requests/my-requests", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<List<object>>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response should indicate success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task GetPendingResponses_ReturnsSuccess_WhenAuthenticated()
        {
            // Arrange
            var (token, _) = await LoginSeededUserAsync(_bobEmail, "Coach");

            // Act
            LogInfo("Getting pending reschedule responses.");
            var response = await _api.GetAsync("/api/v1/reschedule-requests/pending-responses", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<List<object>>(response, true);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response should indicate success");
        }

        private class CreateRescheduleResponseData
        {
            public Guid RequestId { get; set; }
        }
    }
}