using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    // IC-47: realtime function, can not test via unit test
    public class ExecuteTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _existingRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");

        public ExecuteTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task ExecuteTestCases_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("bob@example.com");

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/execute-test-cases", new
            {
                Code = "function add(a, b) { return a + b; }",
                Language = "javascript"
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Execute test cases status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Execute test cases successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ExecuteTestCases_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/execute-test-cases", new
            {
                Code = "function subtract(a, b) { return a - b; }",
                Language = "javascript"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ExecuteTestCases_NonExistentRoom_ReturnsNotFound()
        {
            var token = await LoginUserAsync("bob@example.com");
            var nonExistentRoomId = Guid.NewGuid();

            var response = await _api.PostAsync($"/api/v1/interview-room/{nonExistentRoomId}/execute-test-cases", new
            {
                Code = "function multiply(a, b) { return a * b; }",
                Language = "javascript"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent room ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ExecuteTestCases_MissingCodeOrLanguage_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("bob@example.com");

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/execute-test-cases", new
            {
                Code = "", // Missing code
                Language = "javascript"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing code should return 400 Bad Request");

            response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/execute-test-cases", new
            {
                Code = "function divide(a, b) { return a / b; }",
                Language = "" // Missing language
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing language should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task ExecuteTestCases_UserNotInRoom_ReturnsForbidden()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/execute-test-cases", new
            {
                Code = "function modulo(a, b) { return a % b; }",
                Language = "javascript"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in room should get 403 Forbidden");
        }
    }
}
