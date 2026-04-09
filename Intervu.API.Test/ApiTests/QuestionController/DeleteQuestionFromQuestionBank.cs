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
    public class DeleteQuestionFromQuestionBankTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");

        public DeleteQuestionFromQuestionBankTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid questionId, string userToken)> CreateTestQuestionAsync()
        {
            var email = $"question_user_delete_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Delete Tester", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var token = loginData.Data!.Token;

            var createQuestionResponse = await _api.PostAsync("/api/v1/questions", new CreateQuestionRequest
            {
                Title = "Question to Delete",
                Content = "Content to delete",
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
        public async Task DeleteQuestionFromQuestionBank_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.DeleteAsync($"/api/v1/questions/{Guid.NewGuid()}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task DeleteQuestionFromQuestionBank_Success_ReturnsOk()
        {
            var (questionId, token) = await CreateTestQuestionAsync();

            var response = await _api.DeleteAsync($"/api/v1/questions/{questionId}", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Delete status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Delete request successful");
            await AssertHelper.AssertEqual("Question deleted successfully", payload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task DeleteQuestionFromQuestionBank_NonExistentQuestion_ReturnsNotFound()
        {
            var email = $"user_nonexistent_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "NonExistent User", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;

            var response = await _api.DeleteAsync($"/api/v1/questions/{Guid.NewGuid()}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code 404 for non-existent question");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task DeleteQuestionFromQuestionBank_UnauthorizedUser_ReturnsForbidden()
        {
            var (questionId, _) = await CreateTestQuestionAsync(); // Question created by one user

            var email = $"another_user_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Another User", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var anotherUserToken = (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;

            var response = await _api.DeleteAsync($"/api/v1/questions/{questionId}", jwtToken: anotherUserToken, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code 403 for unauthorized user");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task DeleteQuestionFromQuestionBank_InvalidGuidFormat_ReturnsBadRequest()
        {
            var email = $"user_invalid_guid_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Invalid GUID User", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;

            var response = await _api.DeleteAsync($"/api/v1/questions/not-a-guid", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code 400 for invalid GUID format");
        }
    }
}
