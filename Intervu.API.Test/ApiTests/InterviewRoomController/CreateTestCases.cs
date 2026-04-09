using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class CreateTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _existingRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");

        public CreateTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
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
        public async Task CreateTestCase_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("bob@example.com");

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases", new
            {
                Input = "2 3",
                ExpectedOutput = "5",
                IsPublic = true,
                Explanation = "Basic addition test."
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Create test case status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Create test case successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task CreateTestCase_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases", new
            {
                Input = "1 1",
                ExpectedOutput = "2"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task CreateTestCase_NonExistentRoom_ReturnsNotFound()
        {
            var token = await LoginUserAsync("bob@example.com");
            var nonExistentRoomId = Guid.NewGuid();

            var response = await _api.PostAsync($"/api/v1/interview-room/{nonExistentRoomId}/test-cases", new
            {
                Input = "2 3",
                ExpectedOutput = "5"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent room ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task CreateTestCase_MissingFields_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("bob@example.com");

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases", new
            {
                Input = "", // Missing input
                ExpectedOutput = "" // Missing expected output
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing required fields should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task CreateTestCase_UserNotInRoom_ReturnsForbidden()
        {
            var token = await LoginUserAsync("alice@example.com");

            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases", new
            {
                Input = "2 3",
                ExpectedOutput = "5"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in room should get 403 Forbidden");
        }
    }
}
