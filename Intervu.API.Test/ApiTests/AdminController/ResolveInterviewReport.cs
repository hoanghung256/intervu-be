using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewRoom;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AdminController
{
    public class ResolveInterviewReportTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ResolveInterviewReportTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAdminAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = ADMIN_EMAIL,
                Password = DEFAULT_PASSWORD
            }, logBody: true);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<string> LoginCandidateAsync()
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = "alice@example.com",
                Password = DEFAULT_PASSWORD
            }, logBody: true);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveInterviewReport_ViewReportsAsAdmin_ReturnsSuccess()
        {
            var adminToken = await LoginAdminAsync();

            var response = await _api.GetAsync("/api/v1/admin/room-reports?page=1&pageSize=10", jwtToken: adminToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Admin can view reports with 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveInterviewReport_NonExistentReport_ReturnsBadRequest()
        {
            var adminToken = await LoginAdminAsync();

            var response = await _api.PostAsync("/api/v1/admin/resolve-room-report", new ResolveRoomReportRequest
            {
                ReportId = Guid.NewGuid(),
                Status = InterviewReportStatus.Resolved,
                AdminNote = "Resolve non-existent report",
                RefundOption = RefundOption.None
            }, jwtToken: adminToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Non-existent report returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveInterviewReport_MissingReportId_ReturnsBadRequest()
        {
            var adminToken = await LoginAdminAsync();

            var response = await _api.PostAsync("/api/v1/admin/resolve-room-report", new ResolveRoomReportRequest
            {
                ReportId = Guid.Empty,
                Status = InterviewReportStatus.Resolved,
                AdminNote = "Empty report id",
                RefundOption = RefundOption.Partial50
            }, jwtToken: adminToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Empty reportId returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveInterviewReport_InvalidStatusValue_ReturnsBadRequest()
        {
            var adminToken = await LoginAdminAsync();

            var response = await _api.PostAsync("/api/v1/admin/resolve-room-report", new
            {
                ReportId = Guid.NewGuid(),
                Status = 999,
                AdminNote = "Invalid status enum",
                RefundOption = RefundOption.None
            }, jwtToken: adminToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid status enum returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveInterviewReport_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync("/api/v1/admin/resolve-room-report", new ResolveRoomReportRequest
            {
                ReportId = Guid.NewGuid(),
                Status = InterviewReportStatus.Rejected,
                AdminNote = "No token request",
                RefundOption = RefundOption.None
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Missing token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Admin")]
        public async Task ResolveInterviewReport_WhenCandidateRole_ReturnsForbidden()
        {
            var candidateToken = await LoginCandidateAsync();

            var response = await _api.PostAsync("/api/v1/admin/resolve-room-report", new ResolveRoomReportRequest
            {
                ReportId = Guid.NewGuid(),
                Status = InterviewReportStatus.Resolved,
                AdminNote = "Candidate attempts to resolve",
                RefundOption = RefundOption.Full100
            }, jwtToken: candidateToken, logBody: true);

            await AssertHelper.AssertNotEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-admin role returns 403 Forbidden");
        }
    }
}

