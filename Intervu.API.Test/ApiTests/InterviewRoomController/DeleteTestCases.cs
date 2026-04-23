using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    // IC-45: realtime function, can not test via unit test
    public class DeleteTestCasesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _existingRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");

        public DeleteTestCasesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginUserAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<Guid> CreateTestCaseAndGetIdAsync(string token)
        {
            var response = await _api.PostAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases", new
            {
                Input = "delete-me",
                ExpectedOutput = "deleted"
            }, jwtToken: token);
            var payload = await _api.LogDeserializeJson<JsonElement>(response);
            return payload.Data.GetProperty("id").GetGuid();
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DeleteTestCase_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("bob@example.com");
            var testCaseId = await CreateTestCaseAndGetIdAsync(token);

            var response = await _api.DeleteAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases/{testCaseId}", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Delete test case status is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Delete test case successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DeleteTestCase_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.DeleteAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases/{Guid.NewGuid()}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DeleteTestCase_NonExistentTestCase_ReturnsNotFound()
        {
            var token = await LoginUserAsync("bob@example.com");
            var nonExistentTestCaseId = Guid.NewGuid();

            var response = await _api.DeleteAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases/{nonExistentTestCaseId}", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent test case ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DeleteTestCase_InvalidFormat_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("bob@example.com");

            var response = await _api.DeleteAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases/not-a-guid", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid test case ID format should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DeleteTestCase_UserNotInRoom_ReturnsForbidden()
        {
            var bobToken = await LoginUserAsync("bob@example.com");
            var testCaseId = await CreateTestCaseAndGetIdAsync(bobToken);

            var aliceToken = await LoginUserAsync("alice@example.com");
            var response = await _api.DeleteAsync($"/api/v1/interview-room/{_existingRoomId}/test-cases/{testCaseId}", jwtToken: aliceToken, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in room should get 403 Forbidden");
        }
    }
}
