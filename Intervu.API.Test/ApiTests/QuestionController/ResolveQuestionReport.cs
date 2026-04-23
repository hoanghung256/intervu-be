using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class ResolveQuestionReportTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");

        public ResolveQuestionReportTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAdminAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = ADMIN_EMAIL,
                Password = DEFAULT_PASSWORD
            }, logBody: true);

            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        private async Task<string> RegisterAndLoginCandidateAsync(string emailPrefix)
        {
            var email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com";

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                FullName = "Question Reporter",
                Email = email,
                Password = CANDIDATE_PASSWORD,
                Role = "Candidate"
            }, logBody: true);

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD
            }, logBody: true);

            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        private async Task<Guid> CreateQuestionAsync(string candidateToken)
        {
            var createExperienceResponse = await _api.PostAsync("/api/v1/interview-experiences", new CreateInterviewExperienceRequest
            {
                CompanyId = _googleId,
                Role = "Software Engineer",
                Level = ExperienceLevel.Junior,
                LastRoundCompleted = "Technical",
                InterviewProcess = "Question report integration flow",
                IsInterestedInContact = false
            }, jwtToken: candidateToken, logBody: true);
            var createExperienceData = await _api.LogDeserializeJson<Guid>(createExperienceResponse);

            var createQuestionResponse = await _api.PostAsync($"/api/v1/interview-experiences/{createExperienceData.Data}/questions", new CreateQuestionRequest
            {
                Title = $"Reportable question {Guid.NewGuid():N}",
                Content = "This is a reportable question for integration tests",
                Level = ExperienceLevel.Junior,
                Round = InterviewRound.TechnicalScreen,
                Category = QuestionCategory.Coding,
                CompanyIds = new List<Guid> { _googleId },
                Roles = new List<Role> { Role.SoftwareEngineer },
                TagIds = new List<Guid>()
            }, jwtToken: candidateToken, logBody: true);
            var createQuestionData = await _api.LogDeserializeJson<AddQuestionResult>(createQuestionResponse);

            return createQuestionData.Data!.QuestionId;
        }

        private async Task<Guid> CreateQuestionReportAsync()
        {
            var candidateToken = await RegisterAndLoginCandidateAsync("question_report_candidate");
            var questionId = await CreateQuestionAsync(candidateToken);

            var reportResponse = await _api.PostAsync($"/api/v1/questions/{questionId}/report", new ReportQuestionRequest
            {
                Reason = "Inappropriate wording in the question body"
            }, jwtToken: candidateToken, logBody: true);
            var reportData = await _api.LogDeserializeJson<ReportQuestionResult>(reportResponse, true);

            return reportData.Data!.ReportId;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task ResolveQuestionReport_ReturnsSuccess_WhenAdminResolvesPendingReport()
        {
            var reportId = await CreateQuestionReportAsync();
            var adminToken = await LoginAdminAsync();

            var response = await _api.PutAsync($"/api/v1/questions/reports/{reportId}/status", new UpdateQuestionReportStatusRequest
            {
                Status = QuestionReportStatus.Resolved,
                ActionTaken = ResolutionAction.NoAction,
                ResolutionNote = "Resolved by integration test"
            }, jwtToken: adminToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Admin resolve returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task ResolveQuestionReport_DismissFlow_ReturnsSuccess()
        {
            var reportId = await CreateQuestionReportAsync();
            var adminToken = await LoginAdminAsync();

            var response = await _api.PutAsync($"/api/v1/questions/reports/{reportId}/status", new UpdateQuestionReportStatusRequest
            {
                Status = QuestionReportStatus.Dismissed,
                ActionTaken = ResolutionAction.NoAction,
                ResolutionNote = "Dismissed by integration test"
            }, jwtToken: adminToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Admin dismiss returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task ResolveQuestionReport_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.PutAsync($"/api/v1/questions/reports/{Guid.NewGuid()}/status", new UpdateQuestionReportStatusRequest
            {
                Status = QuestionReportStatus.Resolved,
                ActionTaken = ResolutionAction.NoAction,
                ResolutionNote = "No token test"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Missing token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task ResolveQuestionReport_WhenNonAdminRole_ReturnsForbidden()
        {
            var candidateToken = await RegisterAndLoginCandidateAsync("question_report_non_admin");

            var response = await _api.PutAsync($"/api/v1/questions/reports/{Guid.NewGuid()}/status", new UpdateQuestionReportStatusRequest
            {
                Status = QuestionReportStatus.Resolved,
                ActionTaken = ResolutionAction.NoAction,
                ResolutionNote = "Non-admin test"
            }, jwtToken: candidateToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-admin role returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task ResolveQuestionReport_NonExistentReport_ReturnsNotFound()
        {
            var adminToken = await LoginAdminAsync();

            var response = await _api.PutAsync($"/api/v1/questions/reports/{Guid.NewGuid()}/status", new UpdateQuestionReportStatusRequest
            {
                Status = QuestionReportStatus.Resolved,
                ActionTaken = ResolutionAction.NoAction,
                ResolutionNote = "Non-existent report test"
            }, jwtToken: adminToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent report returns 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task ResolveQuestionReport_InvalidStatusValue_ReturnsBadRequest()
        {
            var adminToken = await LoginAdminAsync();

            var response = await _api.PutAsync($"/api/v1/questions/reports/{Guid.NewGuid()}/status", new
            {
                Status = 999,
                ResolutionNote = "Invalid enum status test"
            }, jwtToken: adminToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid status enum returns 400 BadRequest");
        }
    }
}

