using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AiController
{
    public class CollectQuestionWithAITests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");
        public CollectQuestionWithAITests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        public async Task Handle_AuthenticatedUser_ReturnsGeneratedQuestions()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync($"/api/v1/generated-questions/rooms/{_room1Id}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var body = await _api.LogDeserializeJson<List<GeneratedQuestionDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "Request succeeds");
        }
    }
}
