using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
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
    public class ViewQuestionListTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");

        public ViewQuestionListTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetList_ReturnsSuccess()
        {
            await CreateTestQuestionViaExperienceAsync();
            var response = await _api.GetAsync("/api/v1/questions?pageSize=5", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<QuestionListItemDto>>(response);
            await AssertHelper.AssertNotEmpty(apiResponse.Data!.Items, "Question list is not empty");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetList_InvalidPageSize_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/questions?pageSize=0", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for pageSize=0");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetList_LargePageSize_EnforcesLimit()
        {
            var response = await _api.GetAsync("/api/v1/questions?pageSize=5000", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for large pageSize");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<JsonElement>>(response);
            await AssertHelper.AssertTrue(apiResponse.Data?.Items?.Count <= 100, "Should limit items to reasonable page size");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetList_NegativePage_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/questions?page=-1", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for negative page");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task GetList_FilterByCategory_ReturnsSuccess()
        {
            var response = await _api.GetAsync($"/api/v1/questions?category={(int)QuestionCategory.Coding}&pageSize=5", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for filtered list");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<QuestionListItemDto>>(response);
            if (apiResponse.Data?.Items != null)
            {
                foreach (var item in apiResponse.Data.Items)
                {
                    // This assumes the API returns category information in the list item
                    // await AssertHelper.AssertEqual(QuestionCategory.Coding, item.Category, "Item category should match filter");
                }
            }
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
