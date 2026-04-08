using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using Intervu.Application.DTOs.Feedback;
using Intervu.Domain.Entities;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Feedbacks
{
    public class FeedbacksControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public FeedbacksControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }
        
        // Seeded Data from IntervuPostgreDbContext
        private readonly string _aliceEmail = "alice@example.com";
        private readonly Guid _seededFeedbackId = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10");
        private readonly Guid _feedbackUpdatePendingId = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c11");

        private async Task<(string token, Guid userId)> LoginSeededUserAsync(string email)
        {
            var password = email.Contains("alice") ? DEFAULT_PASSWORD : CANDIDATE_PASSWORD;
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetList_ReturnsSuccess_WhenCandidateIsAuthenticated()
        {
            // Arrange
            var (candidateToken, _) = await LoginSeededUserAsync(_aliceEmail);

            // Act
            LogInfo("Getting feedback list as an authenticated candidate.");
            var response = await _api.GetAsync("/api/v1/feedbacks", jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            // Use the seeded feedback record that is currently empty (Rating 0)
            var (candidateToken, _) = await LoginSeededUserAsync(_aliceEmail);
            var feedbackId = _feedbackUpdatePendingId;

            var updateDto = new UpdateFeedbackDto
            {
                Rating = 5,
                Comments = "Excellent performance! The coach was very helpful."
            };

            // Act
            LogInfo($"Updating feedback {feedbackId} with valid data.");
            var response = await _api.PutAsync($"/api/v1/feedbacks/{feedbackId}", updateDto, jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Feedback update was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenFeedbackAlreadyGiven()
        {
            // Arrange
            // Use the seeded feedback that is already completed (Rating 5)
            var (candidateToken, _) = await LoginSeededUserAsync(_aliceEmail);
            var feedbackId = _seededFeedbackId;

            // Act
            LogInfo("Attempting to update a feedback that has already been completed.");
            var response = await _api.PutAsync($"/api/v1/feedbacks/{feedbackId}", new UpdateFeedbackDto { Rating = 5, Comments = "Trying to update again" }, jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Second feedback update should fail");
            await AssertHelper.AssertContains("already done feedback", apiResponse.Message!, "Message indicates feedback already given");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenRatingIsZero()
        {
            // Arrange
            var (candidateToken, _) = await LoginSeededUserAsync(_aliceEmail);
            var feedbackId = _feedbackUpdatePendingId;

            var updateDto = new UpdateFeedbackDto
            {
                Rating = 0,
                Comments = "Valid comment"
            };

            // Act
            LogInfo("Attempting to update feedback with rating 0.");
            var response = await _api.PutAsync($"/api/v1/feedbacks/{feedbackId}", updateDto, jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Update should fail");
            await AssertHelper.AssertContains("Please input rating", apiResponse.Message!, "Message indicates missing rating");
        }
    }
}