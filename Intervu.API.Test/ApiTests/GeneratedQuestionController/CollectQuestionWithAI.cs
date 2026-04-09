using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.GeneratedQuestionController
{
    // IC-60
    public class CollectQuestionWithAITests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");

        public CollectQuestionWithAITests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public async Task GetByRoom_ReturnsSuccess_WhenAuthenticated()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync($"/api/v1/generated-questions/rooms/{_room1Id}", jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public async Task ApproveAndReject_ReturnsSuccess_WhenAuthorized()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var listResponse = await _api.GetAsync($"/api/v1/generated-questions/rooms/{_room1Id}", jwtToken: loginData.Data!.Token);
            var listData = await _api.LogDeserializeJson<List<GeneratedQuestionDto>>(listResponse);
            if (listData.Data != null && listData.Data.Any())
            {
                var targetQuestion = listData.Data.First();
                var approveResponse = await _api.PutAsync($"/api/v1/generated-questions/{targetQuestion.Id}/approve", new ApproveGeneratedQuestionRequest
                {
                    Title = targetQuestion.Title + " Approved",
                    Content = targetQuestion.Content,
                    Level = Intervu.Domain.Entities.Constants.QuestionConstants.ExperienceLevel.Senior
                }, jwtToken: loginData.Data.Token, logBody: true);
                await AssertHelper.AssertEqual(HttpStatusCode.OK, approveResponse.StatusCode, "Approve status code is 200 OK");
            }
        }
    }
}
