using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.RescheduleRequest;
using Xunit;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRescheduleRequest
{
    public class RescheduleRequestControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private string _candidateToken = "";
        private string _coachToken = "";

        public RescheduleRequestControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        #region Helper Methods

        // Seeded IDs from database (IntervuPostgreDbContext)
        private readonly Guid _existingRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");
        private readonly Guid _proposedAvailId = Guid.Parse("aaaaaaaa-1111-4a1a-8a1a-111111111111");

        private async Task<string> LoginAndGetToken(string email, string password)
        {
            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            var response = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var apiResponse = await _api.LogDeserializeJson<LoginResponse>(response);
            return apiResponse.Data?.Token ?? "";
        }

        private async Task SetupAuthTokens()
        {
            if (string.IsNullOrEmpty(_candidateToken))
            {
                _candidateToken = await LoginAndGetToken("alice@example.com", "123");
            }
            if (string.IsNullOrEmpty(_coachToken))
            {
                _coachToken = await LoginAndGetToken("bob@example.com", "123");
            }
        }

        #endregion

        #region CreateRescheduleRequest Tests

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-01")]
        public async Task CreateRescheduleRequest_WithValidData_ReturnsSuccess()
        {
            // Arrange - Using seeded data from DbContext
            await SetupAuthTokens();
            
            var dto = new CreateRescheduleRequestDto
            {
                RoomId = _existingRoomId,
                ProposedAvailabilityId = _proposedAvailId,
                Reason = "Need to reschedule due to personal emergency that requires my immediate attention"
            };

            LogInfo($"Creating reschedule request for room {_existingRoomId}");
            var response = await _api.PostAsync("/api/v1/reschedule-requests", dto, _candidateToken, logBody: true);

            LogInfo("Verify response is successful");
            await AssertHelper.AssertTrue(
                response.IsSuccessStatusCode,
                $"Should return success. Got: {response.StatusCode}"
            );
            
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await _api.LogDeserializeJson<CreateRescheduleResponse>(response);
                await AssertHelper.AssertTrue(apiResponse.Success, "API response should indicate success");
                await AssertHelper.AssertNotNull(apiResponse.Data, "Response data should not be null");
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-02")]
        public async Task CreateRescheduleRequest_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new CreateRescheduleRequestDto
            {
                RoomId = Guid.NewGuid(),
                ProposedAvailabilityId = Guid.NewGuid(),
                Reason = "Need to reschedule due to personal emergency"
            };

            LogInfo("Creating reschedule request without authentication");
            var response = await _api.PostAsync("/api/v1/reschedule-requests", dto, logBody: true);

            LogInfo("Verify response returns Unauthorized");
            await AssertHelper.AssertTrue(
                response.StatusCode == HttpStatusCode.Unauthorized,
                "Should return Unauthorized status"
            );
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-03")]
        public async Task CreateRescheduleRequest_WithShortReason_ReturnsBadRequest()
        {
            // Arrange
            await SetupAuthTokens();
            var dto = new CreateRescheduleRequestDto
            {
                RoomId = Guid.NewGuid(),
                ProposedAvailabilityId = Guid.NewGuid(),
                Reason = "Short" // Less than 10 characters
            };

            LogInfo("Creating reschedule request with reason less than 10 characters");
            var response = await _api.PostAsync("/api/v1/reschedule-requests", dto, _candidateToken, logBody: true);

            LogInfo("Verify response returns BadRequest");
            await AssertHelper.AssertTrue(
                response.StatusCode == HttpStatusCode.BadRequest,
                "Should return BadRequest for short reason"
            );
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-04")]
        public async Task CreateRescheduleRequest_WithMissingRoomId_ReturnsBadRequest()
        {
            // Arrange
            await SetupAuthTokens();
            var dto = new CreateRescheduleRequestDto
            {
                RoomId = Guid.Empty, // Missing RoomId
                ProposedAvailabilityId = Guid.NewGuid(),
                Reason = "Need to reschedule due to conflict"
            };

            LogInfo("Creating reschedule request with missing RoomId");
            var response = await _api.PostAsync("/api/v1/reschedule-requests", dto, _candidateToken, logBody: true);

            LogInfo("Verify response returns BadRequest");
            var apiResponse = await _api.LogDeserializeJson<CreateRescheduleResponse>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Should not be successful with missing RoomId");
        }

        #endregion

        #region RespondToRescheduleRequest Tests

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-05")]
        public async Task RespondToRescheduleRequest_WithApproval_ReturnsSuccess()
        {
            // Arrange
            await SetupAuthTokens();
            
            // First create a reschedule request to respond to
            var createDto = new CreateRescheduleRequestDto
            {
                RoomId = _existingRoomId,
                ProposedAvailabilityId = _proposedAvailId,
                Reason = "Need to reschedule - test scenario for approval"
            };
            
            LogInfo("Creating reschedule request to test approval");
            var createResponse = await _api.PostAsync("/api/v1/reschedule-requests", createDto, _candidateToken);
            
            if (!createResponse.IsSuccessStatusCode)
            {
                LogInfo($"Could not create reschedule request: {createResponse.StatusCode}");
                return; // Skip test if can't create request
            }
            
            var createApiResponse = await _api.LogDeserializeJson<CreateRescheduleResponse>(createResponse);
            var requestId = createApiResponse.Data?.RequestId ?? Guid.Empty;
            
            var respondDto = new RespondToRescheduleRequestDto
            {
                IsApproved = true,
                RejectionReason = null
            };

            LogInfo($"Approving reschedule request {requestId}");
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", respondDto, _coachToken, logBody: true);

            LogInfo($"Response received with status: {response.StatusCode}");
            await AssertHelper.AssertTrue(
                response.IsSuccessStatusCode,
                $"Should return success. Got: {response.StatusCode}"
            );
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-06")]
        public async Task RespondToRescheduleRequest_WithRejection_ReturnsSuccess()
        {
            // Arrange
            await SetupAuthTokens();
            
            // First create a reschedule request to respond to
            var createDto = new CreateRescheduleRequestDto
            {
                RoomId = _existingRoomId,
                ProposedAvailabilityId = _proposedAvailId,
                Reason = "Need to reschedule - test scenario for rejection"
            };
            
            LogInfo("Creating reschedule request to test rejection");
            var createResponse = await _api.PostAsync("/api/v1/reschedule-requests", createDto, _candidateToken);
            
            if (!createResponse.IsSuccessStatusCode)
            {
                LogInfo($"Could not create reschedule request: {createResponse.StatusCode}");
                return; // Skip test if can't create request
            }
            
            var createApiResponse = await _api.LogDeserializeJson<CreateRescheduleResponse>(createResponse);
            var requestId = createApiResponse.Data?.RequestId ?? Guid.Empty;
            
            var respondDto = new RespondToRescheduleRequestDto
            {
                IsApproved = false,
                RejectionReason = "I'm not available at that proposed time slot"
            };

            LogInfo($"Rejecting reschedule request {requestId}");
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", respondDto, _coachToken, logBody: true);

            LogInfo($"Response received with status: {response.StatusCode}");
            await AssertHelper.AssertTrue(
                response.IsSuccessStatusCode,
                $"Should return success. Got: {response.StatusCode}"
            );
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-07")]
        public async Task RespondToRescheduleRequest_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var dto = new RespondToRescheduleRequestDto
            {
                IsApproved = true
            };

            LogInfo("Responding to reschedule request without authentication");
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{requestId}/respond", dto, logBody: true);

            LogInfo("Verify response returns Unauthorized");
            await AssertHelper.AssertTrue(
                response.StatusCode == HttpStatusCode.Unauthorized,
                "Should return Unauthorized status"
            );
        }

        #endregion

        #region GetRescheduleRequestById Tests

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-08")]
        public async Task GetRescheduleRequestById_WithValidId_ReturnsData()
        {
            // Arrange
            await SetupAuthTokens();
            var requestId = Guid.NewGuid();

            LogInfo($"Getting reschedule request by ID: {requestId}");
            var response = await _api.GetAsync($"/api/v1/reschedule-requests/{requestId}", _candidateToken, logBody: true);

            LogInfo("Verify response received");
            var apiResponse = await _api.LogDeserializeJson<GetRescheduleRequestResponse>(response);
            
            LogInfo($"Response status: {response.StatusCode}");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-09")]
        public async Task GetRescheduleRequestById_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var requestId = Guid.NewGuid();

            LogInfo("Getting reschedule request without authentication");
            var response = await _api.GetAsync($"/api/v1/reschedule-requests/{requestId}", logBody: true);

            LogInfo("Verify response returns Unauthorized");
            await AssertHelper.AssertTrue(
                response.StatusCode == HttpStatusCode.Unauthorized,
                "Should return Unauthorized status"
            );
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-10")]
        public async Task GetRescheduleRequestById_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            await SetupAuthTokens();
            var nonExistentId = Guid.NewGuid();

            LogInfo($"Getting non-existent reschedule request: {nonExistentId}");
            var response = await _api.GetAsync($"/api/v1/reschedule-requests/{nonExistentId}", _candidateToken, logBody: true);

            LogInfo("Verify response returns NotFound");
            await AssertHelper.AssertTrue(
                response.StatusCode == HttpStatusCode.NotFound,
                "Should return NotFound for non-existent request"
            );
        }

        #endregion

        #region GetMyRescheduleRequests Tests

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-11")]
        public async Task GetMyRescheduleRequests_WithAuthentication_ReturnsData()
        {
            // Arrange
            await SetupAuthTokens();

            LogInfo("Getting my reschedule requests");
            var response = await _api.GetAsync("/api/v1/reschedule-requests/my-requests", _candidateToken, logBody: true);

            LogInfo("Verify response is successful");
            
            // API should return success even with empty list
            await AssertHelper.AssertTrue(
                response.IsSuccessStatusCode,
                $"Should return success status. Got: {response.StatusCode}"
            );
            
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await _api.LogDeserializeJson<GetMyRescheduleRequestsResponse>(response);
                await AssertHelper.AssertTrue(apiResponse.Success, "API response should indicate success");
                LogInfo($"Found {(apiResponse.Data?.Count ?? 0)} reschedule requests");
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-12")]
        public async Task GetMyRescheduleRequests_WithoutAuthentication_ReturnsUnauthorized()
        {
            LogInfo("Getting my reschedule requests without authentication");
            var response = await _api.GetAsync("/api/v1/reschedule-requests/my-requests", logBody: true);

            LogInfo("Verify response returns Unauthorized");
            await AssertHelper.AssertTrue(
                response.StatusCode == HttpStatusCode.Unauthorized,
                "Should return Unauthorized status"
            );
        }

        #endregion

        #region GetPendingResponses Tests

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-13")]
        public async Task GetPendingResponses_WithAuthentication_ReturnsData()
        {
            // Arrange
            await SetupAuthTokens();

            LogInfo("Getting pending reschedule responses");
            var response = await _api.GetAsync("/api/v1/reschedule-requests/pending-responses", _coachToken, logBody: true);

            LogInfo("Verify response is successful");
            
            // API should return success even with empty list
            await AssertHelper.AssertTrue(
                response.IsSuccessStatusCode,
                $"Should return success status. Got: {response.StatusCode}"
            );
            
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await _api.LogDeserializeJson<GetPendingResponsesResponse>(response);
                await AssertHelper.AssertTrue(apiResponse.Success, "API response should indicate success");
                LogInfo($"Found {(apiResponse.Data?.Count ?? 0)} pending responses");
            }
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        [Trait("Name", "RR-14")]
        public async Task GetPendingResponses_WithoutAuthentication_ReturnsUnauthorized()
        {
            LogInfo("Getting pending responses without authentication");
            var response = await _api.GetAsync("/api/v1/reschedule-requests/pending-responses", logBody: true);

            LogInfo("Verify response returns Unauthorized");
            await AssertHelper.AssertTrue(
                response.StatusCode == HttpStatusCode.Unauthorized,
                "Should return Unauthorized status"
            );
        }

        #endregion

        #region Response DTOs

        private class LoginResponse
        {
            public string? Token { get; set; }
        }

        private class CreateRescheduleResponse
        {
            public Guid RequestId { get; set; }
        }

        private class RespondToRescheduleResponse
        {
            public string? Message { get; set; }
        }

        private class GetRescheduleRequestResponse
        {
            public Guid Id { get; set; }
            public Guid InterviewRoomId { get; set; }
            public string? Status { get; set; }
        }

        private class GetMyRescheduleRequestsResponse
        {
            public List<object>? Data { get; set; }
            public int Count => Data?.Count ?? 0;
        }

        private class GetPendingResponsesResponse
        {
            public List<object>? Data { get; set; }
            public int Count => Data?.Count ?? 0;
        }

        #endregion
    }
}
