using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;
using Intervu.Domain.Entities.Constants;
using Intervu.Application.DTOs.Common;
using Intervu.Domain.Entities.Constants.QuestionConstants;

namespace Intervu.API.Test.ApiTests.InterviewExperience
{
    // TODO: split to multiple test classes - Create experience, Get experience detail, Get experience list, Update experience, Delete experience, Add question to experience
    public class InterviewExperienceControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public InterviewExperienceControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginUserAsync()
        {
            var email = $"experience_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Experience User",
                Role = "Candidate"
            });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        // Seeded Company ID
        private readonly Guid _googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewExperience")]
        public async Task InterviewExperience_Lifecycle_ReturnsSuccess()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginUserAsync();

            // 1. Create Experience
            var createRequest = new CreateInterviewExperienceRequest
            {
                CompanyId = _googleId,
                Role = "Software Engineer",
                Level = ExperienceLevel.Senior,
                LastRoundCompleted = "Onsite",
                InterviewProcess = "Initial phone screen followed by onsite technical rounds.",
                IsInterestedInContact = true,
                Questions = new List<CreateQuestionRequest>()
            };

            LogInfo("Submitting interview experience.");
            var createResponse = await _api.PostAsync("/api/v1/interview-experiences", createRequest, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");
            var createResult = await _api.LogDeserializeJson<Guid>(createResponse);
            var experienceId = createResult.Data;

            // 2. Get Detail
            LogInfo($"Getting details for experience {experienceId}.");
            var getDetailResponse = await _api.GetAsync($"/api/v1/interview-experiences/{experienceId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getDetailResponse.StatusCode, "Get detail status code is 200 OK");
            var getDetailData = await _api.LogDeserializeJson<InterviewExperienceDetailDto>(getDetailResponse);
            await AssertHelper.AssertEqual(createRequest.Role, getDetailData.Data!.Role, "Role matches");

            // 3. Get List (with filter)
            LogInfo("Getting interview experiences list.");
            var listResponse = await _api.GetAsync($"/api/v1/interview-experiences?companyId={_googleId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, listResponse.StatusCode, "Get list status code is 200 OK");
            var listData = await _api.LogDeserializeJson<PagedResult<InterviewExperienceSummaryDto>>(listResponse);
            await AssertHelper.AssertTrue(listData.Data!.Items.Any(e => e.Id == experienceId), "Created experience found in list");

            // 4. Update
            var updateRequest = new UpdateInterviewExperienceRequest
            {
                CompanyId = _googleId,
                Role = "Senior Software Engineer",
                Level = ExperienceLevel.Senior,
                LastRoundCompleted = "Hired",
                InterviewProcess = "Updated process description.",
                IsInterestedInContact = false
            };
            LogInfo($"Updating experience {experienceId}.");
            var updateResponse = await _api.PutAsync($"/api/v1/interview-experiences/{experienceId}", updateRequest, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");

            // 5. Add Question to Experience
            var questionRequest = new CreateQuestionRequest
            {
                Title = "New Question Title",
                Content = "Question content asked in this interview.",
                Level = ExperienceLevel.Senior,
                Round = Intervu.Domain.Entities.Constants.QuestionConstants.InterviewRound.TechnicalScreen,
                Category = QuestionCategory.Coding,
                CompanyIds = new List<Guid> { _googleId },
                Roles = new List<Role> { Role.SoftwareEngineer },
                TagIds = new List<Guid>()
            };
            LogInfo($"Adding question to experience {experienceId}.");
            var addQuestionResponse = await _api.PostAsync($"/api/v1/interview-experiences/{experienceId}/questions", questionRequest, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, addQuestionResponse.StatusCode, "Add question status code is 200 OK");
            var addQuestionResult = await _api.LogDeserializeJson<AddQuestionResult>(addQuestionResponse);
            await AssertHelper.AssertNotNull(addQuestionResult.Data, "Add question result is not null");

            // 6. Delete
            LogInfo($"Deleting experience {experienceId}.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/interview-experiences/{experienceId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewExperience")]
        public async Task GetDetail_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Act
            var randomId = Guid.NewGuid();
            LogInfo($"Getting non-existent experience {randomId}.");
            var response = await _api.GetAsync($"/api/v1/interview-experiences/{randomId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
        }
    }
}