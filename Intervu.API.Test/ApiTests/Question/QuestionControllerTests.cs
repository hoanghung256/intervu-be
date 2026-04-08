using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;
using Intervu.Application.DTOs.Common;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Application.DTOs.InterviewExperience;

namespace Intervu.API.Test.ApiTests.Question
{
    public class QuestionControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public QuestionControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginUserAsync()
        {
            var email = $"question_user_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Question Tester",
                Role = "Candidate"
            });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<string> LoginAsAdminAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        // Seeded Company ID for creating InterviewExperience
        private readonly Guid _googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");

        private async Task<(Guid questionId, string userToken)> CreateTestQuestionViaExperienceAsync(string title = "Dynamic Test Question", string content = "Dynamic Test Content")
        {
            var (userToken, userId) = await RegisterAndLoginUserAsync();

            // 1. Create Interview Experience
            var createExperienceRequest = new CreateInterviewExperienceRequest
            {
                CompanyId = _googleId,
                Role = "Software Engineer",
                Level = ExperienceLevel.Junior,
                LastRoundCompleted = "Technical",
                InterviewProcess = "Dynamic test experience process.",
                IsInterestedInContact = false
            };

            var createExperienceResponse = await _api.PostAsync("/api/v1/interview-experiences", createExperienceRequest, jwtToken: userToken);
            var createExperienceResult = await _api.LogDeserializeJson<Guid>(createExperienceResponse);
            var experienceId = createExperienceResult.Data;

            // 2. Add Question to Experience
            var questionRequest = new CreateQuestionRequest
            {
                Title = title,
                Content = content,
                Level = ExperienceLevel.Junior,
                Round = InterviewRound.TechnicalScreen,
                Category = QuestionCategory.Coding,
                CompanyIds = new List<Guid> { _googleId },
                Roles = new List<Role> { Role.SoftwareEngineer },
                TagIds = new List<Guid>()
            };
            var addQuestionResponse = await _api.PostAsync($"/api/v1/interview-experiences/{experienceId}/questions", questionRequest, jwtToken: userToken);
            var addQuestionResult = await _api.LogDeserializeJson<AddQuestionResult>(addQuestionResponse);
            var questionId = addQuestionResult.Data!.QuestionId;

            return (questionId, userToken);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetList_ReturnsSuccess()
        {
            // Arrange: Create a question to ensure there's at least one dynamic entry
            await CreateTestQuestionViaExperienceAsync();

            // Act
            LogInfo("Getting question list.");
            var response = await _api.GetAsync("/api/v1/questions?pageSize=5", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<QuestionListItemDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertNotEmpty(apiResponse.Data!.Items, "Question list is not empty");
        }

//         [Fact]
//         [Trait("Category", "API")]
//         [Trait("Category", "Question")]
//         public async Task GetDetail_ReturnsSuccess_WhenIdIsValid()
//         {
//             // Arrange
//             var (questionId, _) = await CreateTestQuestionViaExperienceAsync();
//
//             // Act
//             LogInfo($"Getting details for question {questionId}.");
//             var response = await _api.GetAsync($"/api/v1/questions/{questionId}", logBody: true);
//
//             // Assert
//             await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
//             var apiResponse = await _api.LogDeserializeJson<QuestionDetailDto>(response);
//             await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
//             await AssertHelper.AssertEqual(questionId, apiResponse.Data!.Id, "Question ID matches");
//         }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task Search_ReturnsSuccess_WhenKeywordProvided()
        {
            // Arrange
            var (questionId, _) = await CreateTestQuestionViaExperienceAsync("Searchable Question Title", "Content for searching");
            var keyword = "Searchable";

            // Act
            LogInfo($"Searching for questions with keyword: {keyword}");
            var response = await _api.GetAsync($"/api/v1/questions/search?keyword={keyword}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<List<QuestionSearchResultDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertTrue(apiResponse.Data!.Any(q => q.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)), "Found question matching keyword");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task QuestionInteractions_LikeAndSave_ReturnsSuccess()
        {
            // Arrange
            var (questionId, userToken) = await CreateTestQuestionViaExperienceAsync();

            // 1. Like
            LogInfo($"Liking question {questionId}.");
            var likeResponse = await _api.PostAsync($"/api/v1/questions/{questionId}/like", new { }, jwtToken: userToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, likeResponse.StatusCode, "Like status code is 200 OK");

            // 2. Save
            LogInfo($"Saving question {questionId}.");
            var saveResponse = await _api.PostAsync($"/api/v1/questions/{questionId}/save", true, jwtToken: userToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, saveResponse.StatusCode, "Save status code is 200 OK");

            // 3. Get Saved
            LogInfo("Getting my saved questions.");
            var savedListResponse = await _api.GetAsync("/api/v1/questions/saved", jwtToken: userToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, savedListResponse.StatusCode, "Get saved status code is 200 OK");
            var savedData = await _api.LogDeserializeJson<List<QuestionListItemDto>>(savedListResponse);
            await AssertHelper.AssertTrue(savedData.Data!.Any(q => q.Id == questionId), "Question is in my saved list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task ManageQuestionReports_AdminFlow_ReturnsSuccess()
        {
            // Arrange
            var (questionId, userToken) = await CreateTestQuestionViaExperienceAsync();
            var adminToken = await LoginAsAdminAsync();

            // 1. Report Question
            var reportRequest = new ReportQuestionRequest
            {
                Reason = "Incorrect content"
            };
            LogInfo($"Reporting question {questionId} as user.");
            var reportResponse = await _api.PostAsync($"/api/v1/questions/{questionId}/report", reportRequest, jwtToken: userToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, reportResponse.StatusCode, "Report status code is 200 OK");
            var reportResult = await _api.LogDeserializeJson<ReportQuestionResult>(reportResponse);
            var reportId = reportResult.Data!.ReportId;

            // 2. Admin Get Reports
            LogInfo("Admin getting question reports.");
            var listReportsResponse = await _api.GetAsync("/api/v1/questions/reports", jwtToken: adminToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, listReportsResponse.StatusCode, "Admin get reports status code is 200 OK");
            var listReportsData = await _api.LogDeserializeJson<PagedResult<QuestionReportItemDto>>(listReportsResponse);
            await AssertHelper.AssertTrue(listReportsData.Data!.Items.Any(r => r.Id == reportId), "Report is in the admin list");

            // 3. Admin Update Report Status
            var updateStatusRequest = new UpdateQuestionReportStatusRequest
            {
                Status = QuestionReportStatus.Reviewed
            };
            LogInfo($"Admin resolving report {reportId}.");
            var updateResponse = await _api.PutAsync($"/api/v1/questions/reports/{reportId}/status", updateStatusRequest, jwtToken: adminToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update report status code is 200 OK");
        }
    }
}