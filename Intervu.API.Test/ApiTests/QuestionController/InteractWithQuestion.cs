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
    public class InteractWithQuestionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");

        public InteractWithQuestionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> RegisterAndLoginCandidateAsync(string emailPrefix)
        {
            var email = $"{emailPrefix}_{Guid.NewGuid():N}@example.com";

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                FullName = "Question Interaction User",
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
                InterviewProcess = "Interaction flow setup",
                IsInterestedInContact = false
            }, jwtToken: candidateToken, logBody: true);
            var createExperienceData = await _api.LogDeserializeJson<Guid>(createExperienceResponse);

            var createQuestionResponse = await _api.PostAsync($"/api/v1/interview-experiences/{createExperienceData.Data}/questions", new CreateQuestionRequest
            {
                Title = $"Interactive question {Guid.NewGuid():N}",
                Content = "This question is used to test like/save/report interactions",
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

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task InteractWithQuestion_Like_ReturnsSuccess()
        {
            var token = await RegisterAndLoginCandidateAsync("question_like");
            var questionId = await CreateQuestionAsync(token);

            var response = await _api.PostAsync<object>($"/api/v1/questions/{questionId}/like", null, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Like question returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task InteractWithQuestion_Save_ReturnsSuccess()
        {
            var token = await RegisterAndLoginCandidateAsync("question_save");
            var questionId = await CreateQuestionAsync(token);

            var response = await _api.PostAsync($"/api/v1/questions/{questionId}/save", true, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Save question returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task InteractWithQuestion_Unsave_ReturnsSuccess()
        {
            var token = await RegisterAndLoginCandidateAsync("question_unsave");
            var questionId = await CreateQuestionAsync(token);

            var saveResponse = await _api.PostAsync($"/api/v1/questions/{questionId}/save", true, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, saveResponse.StatusCode, "Precondition save returns 200 OK");

            var response = await _api.PostAsync($"/api/v1/questions/{questionId}/save", false, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Unsave question returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task InteractWithQuestion_Report_ReturnsSuccess()
        {
            var token = await RegisterAndLoginCandidateAsync("question_report");
            var questionId = await CreateQuestionAsync(token);

            var response = await _api.PostAsync($"/api/v1/questions/{questionId}/report", new ReportQuestionRequest
            {
                Reason = "This question contains confusing details"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Report question returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task InteractWithQuestion_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync<object>($"/api/v1/questions/{Guid.NewGuid()}/like", null, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Missing token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task InteractWithQuestion_SaveNonExistentQuestion_ReturnsInternalServerError()
        {
            var token = await RegisterAndLoginCandidateAsync("question_save_nonexistent");

            var response = await _api.PostAsync($"/api/v1/questions/{Guid.NewGuid()}/save", true, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Current behavior returns 500 for non-existent question");
        }
    }
}

