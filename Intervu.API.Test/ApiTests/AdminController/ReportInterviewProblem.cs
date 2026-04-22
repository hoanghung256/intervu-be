using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    // TODO: Add UC into doc - View all interview reports
    public class ReportInterviewProblemTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public ReportInterviewProblemTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<string> LoginUserAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task Handle_AdminRequest_ReturnsRoomReports()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/room-reports", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveRoomReport_NonExistentReport_ReturnsBadRequest()
        {
            // Arrange
            var token = await LoginAdminAsync();
            var request = new ResolveRoomReportRequest
            {
                ReportId = Guid.NewGuid(),
                Status = InterviewReportStatus.Resolved,
                AdminNote = "Test resolution",
                RefundOption = RefundOption.None
            };

            // Act
            var response = await _api.PostAsync("/api/v1/admin/resolve-room-report", request, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Non-existent report returns 400 BadRequest");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task Handle_ZeroPagingParams_ReturnsSuccess()
        {
            // Arrange
            var token = await LoginAdminAsync();

            // Act
            var response = await _api.GetAsync("/api/v1/admin/room-reports?page=0&pageSize=0", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task Handle_NonAdminRequest_ReturnsForbidden()
        {
            // Arrange
            var token = await LoginUserAsync("alice@example.com");

            // Act
            var response = await _api.GetAsync("/api/v1/admin/room-reports", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task Handle_NoToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _api.GetAsync("/api/v1/admin/room-reports", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveRoomReport_WithoutAuthToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new ResolveRoomReportRequest
            {
                ReportId = Guid.NewGuid(),
                Status = InterviewReportStatus.Resolved,
                AdminNote = "Test",
                RefundOption = RefundOption.None
            };

            // Act
            var response = await _api.PostAsync("/api/v1/admin/resolve-room-report", request, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token resolve returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveRoomReport_NonAdmin_ReturnsForbidden()
        {
            // Arrange
            var token = await LoginUserAsync("alice@example.com");
            var request = new ResolveRoomReportRequest
            {
                ReportId = Guid.NewGuid(),
                Status = InterviewReportStatus.Resolved,
                AdminNote = "Test",
                RefundOption = RefundOption.None
            };

            // Act
            var response = await _api.PostAsync("/api/v1/admin/resolve-room-report", request, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-admin resolve returns 403 Forbidden");
        }
    }
}
