using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    // IC-43: realtime function, can not test via unit test
    public class UseLiveCodingEditorTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _existingRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"); // Assuming this is a valid room ID for testing

        public UseLiveCodingEditorTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginUserAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task UpdateEditorContent_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("bob@example.com"); // Assuming Bob is in the room

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/editor-content", new
            {
                Content = "console.log('Hello World');",
                Language = "javascript"
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Update editor content status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Update editor content successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task UpdateEditorContent_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/editor-content", new
            {
                Content = "console.log('Unauthorized');",
                Language = "javascript"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task UpdateEditorContent_NonExistentRoom_ReturnsNotFound()
        {
            var token = await LoginUserAsync("bob@example.com");
            var nonExistentRoomId = Guid.NewGuid();

            var response = await _api.PostAsync($"/api/v1/interview-room/{nonExistentRoomId}/editor-content", new
            {
                Content = "console.log('Non-existent room');",
                Language = "javascript"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent room ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task UpdateEditorContent_MissingContentOrLanguage_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("bob@example.com");

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/editor-content", new
            {
                Content = "", // Missing content
                Language = "python"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing content should return 400 Bad Request");

            response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/editor-content", new
            {
                Content = "print('Hello');",
                Language = "" // Missing language
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing language should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task UpdateEditorContent_UserNotInRoom_ReturnsForbidden()
        {
            var token = await LoginUserAsync("alice@example.com"); // Assuming Alice is NOT in the room

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/editor-content", new
            {
                Content = "console.log('Forbidden');",
                Language = "javascript"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in room should get 403 Forbidden");
        }
    }
}
