using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    // IC-59
    public class ViewQuestionDetailTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");

        public ViewQuestionDetailTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetDetail_ReturnsNotFound_WhenQuestionDoesNotExist()
        {
            var response = await _api.GetAsync($"/api/v1/questions/{Guid.NewGuid()}", logBody: true);
            var apiResponse = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 NotFound");
            await AssertHelper.AssertFalse(apiResponse.Success, "Response indicates failure");
            await AssertHelper.AssertEqual("Question not found", apiResponse.Message, "Error message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetDetail_ReturnsSuccess_WhenQuestionExists()
        {
            var (questionId, _) = await CreateTestQuestionViaExperienceAsync();
            var response = await _api.GetAsync($"/api/v1/questions/{questionId}", logBody: true);
            var apiResponse = await _api.LogDeserializeJson<QuestionDetailDto>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Question detail data is returned");
            await AssertHelper.AssertEqual(questionId, apiResponse.Data!.Id, "Returned question ID matches requested ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetDetail_InvalidIdFormat_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/questions/invalid-guid-format", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid GUID format");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetDetail_QuestionWithNoTags_ReturnsSuccess()
        {
            var email = $"question_user_notags_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "No Tags Tester", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var token = loginData.Data!.Token;

            var createExperienceResponse = await _api.PostAsync("/api/v1/interview-experiences", new CreateInterviewExperienceRequest
            {
                CompanyId = _googleId,
                Role = "Software Engineer",
                Level = ExperienceLevel.Junior,
                LastRoundCompleted = "Technical",
                InterviewProcess = "Dynamic test experience process.",
                IsInterestedInContact = false
            }, jwtToken: token);
            var createExperienceResult = await _api.LogDeserializeJson<Guid>(createExperienceResponse);

            var addQuestionResponse = await _api.PostAsync($"/api/v1/interview-experiences/{createExperienceResult.Data}/questions", new CreateQuestionRequest
            {
                Title = "Question with No Tags",
                Content = "Content for no tags question",
                Level = ExperienceLevel.Junior,
                Round = InterviewRound.TechnicalScreen,
                Category = QuestionCategory.Coding,
                CompanyIds = new List<Guid> { _googleId },
                Roles = new List<Role> { Role.SoftwareEngineer },
                TagIds = new List<Guid>() // No tags
            }, jwtToken: token);
            var addQuestionResult = await _api.LogDeserializeJson<AddQuestionResult>(addQuestionResponse);
            var questionId = addQuestionResult.Data!.QuestionId;

            var response = await _api.GetAsync($"/api/v1/questions/{questionId}", logBody: true);
            var apiResponse = await _api.LogDeserializeJson<QuestionDetailDto>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Response indicates success");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Question detail data is returned");
            await AssertHelper.AssertEmpty(apiResponse.Data!.Tags, "Tags list should be empty");
        }

        private async Task<(Guid questionId, string userToken)> CreateTestQuestionViaExperienceAsync()
        {
            var email = $"question_user_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Question Tester", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var token = loginData.Data!.Token;

            var createExperienceResponse = await _api.PostAsync("/api/v1/interview-experiences", new CreateInterviewExperienceRequest
            {
                CompanyId = _googleId,
                Role = "Software Engineer",
                Level = ExperienceLevel.Junior,
                LastRoundCompleted = "Technical",
                InterviewProcess = "Dynamic test experience process.",
                IsInterestedInContact = false
            }, jwtToken: token);
            var createExperienceResult = await _api.LogDeserializeJson<Guid>(createExperienceResponse);

            var addQuestionResponse = await _api.PostAsync($"/api/v1/interview-experiences/{createExperienceResult.Data}/questions", new CreateQuestionRequest
            {
                Title = "Dynamic Test Question",
                Content = "Dynamic Test Content",
                Level = ExperienceLevel.Junior,
                Round = InterviewRound.TechnicalScreen,
                Category = QuestionCategory.Coding,
                CompanyIds = new List<Guid> { _googleId },
                Roles = new List<Role> { Role.SoftwareEngineer },
                TagIds = new List<Guid>()
            }, jwtToken: token);
            var addQuestionResult = await _api.LogDeserializeJson<AddQuestionResult>(addQuestionResponse);
            return (addQuestionResult.Data!.QuestionId, token);
        }
    }
}
