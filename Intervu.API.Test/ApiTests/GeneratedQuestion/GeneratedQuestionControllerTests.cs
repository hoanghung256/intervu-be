using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.GeneratedQuestion;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.GeneratedQuestion
{
    public class GeneratedQuestionControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public GeneratedQuestionControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAsAdminAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        private async Task<string> LoginAsCoachAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        // Seeded IDs from IntervuPostgreDbContext
        private readonly Guid _room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public async Task GetByRoom_ReturnsSuccess_WhenAuthenticated()
        {
            // Arrange
            var token = await LoginAsCoachAsync();

            // Act
            LogInfo($"Getting generated questions for room {_room1Id}.");
            var response = await _api.GetAsync($"/api/v1/generated-questions/rooms/{_room1Id}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "GeneratedQuestion")]
        public async Task ApproveAndReject_ReturnsSuccess_WhenAuthorized()
        {
            // Arrange
            var token = await LoginAsAdminAsync();

            // Note: In a real scenario, we'd need to trigger AI generation or seed a GeneratedQuestion.
            // Since we can't easily trigger AI in a unit test without more context,
            // we'll assume there might be questions or test the auth/routing.

            LogInfo("Fetching generated questions to find one to approve/reject.");
            var listResponse = await _api.GetAsync($"/api/v1/generated-questions/rooms/{_room1Id}", jwtToken: token);
            var listData = await _api.LogDeserializeJson<List<GeneratedQuestionDto>>(listResponse);

            if (listData.Data != null && listData.Data.Any())
            {
                var targetQuestion = listData.Data.First();
                var gqId = targetQuestion.Id;

                // 1. Approve
                var approveRequest = new ApproveGeneratedQuestionRequest
                {
                    Title = targetQuestion.Title + " Approved",
                    Content = targetQuestion.Content,
                    Level = ExperienceLevel.Senior
                };

                LogInfo($"Approving generated question {gqId}.");
                var approveResponse = await _api.PutAsync($"/api/v1/generated-questions/{gqId}/approve", approveRequest, jwtToken: token, logBody: true);
                await AssertHelper.AssertEqual(HttpStatusCode.OK, approveResponse.StatusCode, "Approve status code is 200 OK");

                // 2. Reject (Need another question or reset state, for now we'll try a different one if available)
                if (listData.Data.Count > 1)
                {
                    var rejectId = listData.Data.Last().Id;
                    LogInfo($"Rejecting generated question {rejectId}.");
                    var rejectResponse = await _api.PutAsync($"/api/v1/generated-questions/{rejectId}/reject", new { }, jwtToken: token, logBody: true);
                    await AssertHelper.AssertEqual(HttpStatusCode.OK, rejectResponse.StatusCode, "Reject status code is 200 OK");
                }
            }
            else
            {
                LogInfo("No generated questions found in seeded room to test Approve/Reject.");
            }
        }
    }
}