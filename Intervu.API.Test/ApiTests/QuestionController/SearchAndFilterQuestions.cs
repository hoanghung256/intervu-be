using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Question;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.QuestionController
{
    // IC-24
    public class SearchAndFilterQuestionsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SearchAndFilterQuestionsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task Search_ReturnsSuccess_WhenKeywordProvided()
        {
            var response = await _api.GetAsync("/api/v1/questions/search?keyword=Searchable", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<List<QuestionSearchResultDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task Search_ExcludesPendingQuestions()
        {
            var uniqueMarker = $"Pendmark{Guid.NewGuid():N}".Substring(0, 24);

            var email = $"searcher_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Searcher", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;

            var googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");
            await _api.PostAsync("/api/v1/questions", new CreateQuestionRequest
            {
                Title = $"Pending search test {uniqueMarker}",
                Content = $"Body for {uniqueMarker}",
                Level = ExperienceLevel.Junior,
                Round = InterviewRound.TechnicalScreen,
                Category = QuestionCategory.Coding,
                CompanyIds = new List<Guid> { googleId },
                Roles = new List<Role> { Role.SoftwareEngineer },
                TagIds = new List<Guid>()
            }, jwtToken: token);

            var response = await _api.GetAsync($"/api/v1/questions/search?keyword={uniqueMarker}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Search status code is 200 OK");

            var payload = await _api.LogDeserializeJson<List<QuestionSearchResultDto>>(response);
            var items = payload.Data ?? new List<QuestionSearchResultDto>();
            await AssertHelper.AssertEqual(0, items.Count, "Pending questions are not surfaced in public search");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Question")]
        public async Task Search_ReturnsEmpty_WhenKeywordIsWhitespace()
        {
            var response = await _api.GetAsync("/api/v1/questions/search?keyword=%20", logBody: true);
            var content = await response.Content.ReadAsStringAsync();

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertContains("keyword", content, "Validation message contains keyword field");
        }
    }
}
