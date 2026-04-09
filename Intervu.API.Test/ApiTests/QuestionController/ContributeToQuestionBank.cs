using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    public class ContributeToQuestionBankTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");

        public ContributeToQuestionBankTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid questionId, string userToken)> CreateTestQuestionAsync()
        {
            var email = $"contributor_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Contributor", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var token = loginData.Data!.Token;

            var createQuestionResponse = await _api.PostAsync("/api/v1/questions", new CreateQuestionRequest
            {
                Title = "Question to Contribute",
                Content = "Contribution content",
                Level = ExperienceLevel.Junior,
                Round = InterviewRound.TechnicalScreen,
                Category = QuestionCategory.Coding,
                CompanyIds = new List<Guid> { _googleId },
                Roles = new List<Role> { Role.SoftwareEngineer },
                TagIds = new List<Guid>()
            }, jwtToken: token);
            var createQuestionResult = await _api.LogDeserializeJson<AddQuestionResult>(createQuestionResponse);
            return (createQuestionResult.Data!.QuestionId, token);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task SaveQuestion_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.PostAsync($"/api/v1/questions/{Guid.NewGuid()}/save", true, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task SaveQuestion_Success_ReturnsOk()
        {
            var (questionId, token) = await CreateTestQuestionAsync();

            var response = await _api.PostAsync($"/api/v1/questions/{questionId}/save", true, jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Save status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Save request successful");
            await AssertHelper.AssertEqual("Question saved successfully", payload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task SaveQuestion_NonExistentQuestion_ReturnsNotFound()
        {
            var email = $"contributor_notfound_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Contributor NotFound", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;

            var response = await _api.PostAsync($"/api/v1/questions/{Guid.NewGuid()}/save", true, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code 404 for non-existent question");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task SaveQuestion_DuplicateSave_ReturnsConflict()
        {
            var (questionId, token) = await CreateTestQuestionAsync();

            // First save
            await _api.PostAsync($"/api/v1/questions/{questionId}/save", true, jwtToken: token, logBody: true);

            // Second save
            var response = await _api.PostAsync($"/api/v1/questions/{questionId}/save", true, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, response.StatusCode, "Status code 409 for duplicate save");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task SaveQuestion_Unsave_ReturnsOk()
        {
            var (questionId, token) = await CreateTestQuestionAsync();

            // First save
            await _api.PostAsync($"/api/v1/questions/{questionId}/save", true, jwtToken: token, logBody: true);

            // Unsave
            var response = await _api.PostAsync($"/api/v1/questions/{questionId}/save", false, jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Unsave status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Unsave request successful");
            await AssertHelper.AssertEqual("Question unsaved successfully", payload.Message, "Success message matches");
        }
    }
}
