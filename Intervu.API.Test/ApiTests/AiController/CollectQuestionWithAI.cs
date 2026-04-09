using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AiController
{
    // IC-60
    public class CollectQuestionWithAITests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");
        public CollectQuestionWithAITests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task Handle_AuthenticatedUser_ReturnsGeneratedQuestions()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync($"/api/v1/generated-questions/rooms/{_room1Id}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var body = await _api.LogDeserializeJson<List<GeneratedQuestionDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "Request succeeds");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task Handle_MissingToken_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync($"/api/v1/generated-questions/rooms/{_room1Id}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task Handle_InvalidRoomId_ReturnsNotFound()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var nonExistentRoomId = Guid.NewGuid();
            var response = await _api.GetAsync($"/api/v1/generated-questions/rooms/{nonExistentRoomId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent room ID returns 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task Handle_UnauthorizedUserForRoom_ReturnsForbidden()
        {
            // Assuming alice is not part of room1
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync($"/api/v1/generated-questions/rooms/{_room1Id}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in room returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task Handle_RoomIdFormatInvalid_ReturnsBadRequest()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var invalidRoomId = "not-a-guid";
            var response = await _api.GetAsync($"/api/v1/generated-questions/rooms/{invalidRoomId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid room ID format returns 400 Bad Request");
        }
    }
}
