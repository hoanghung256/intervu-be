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
        private readonly Guid _aliceId = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
        private readonly string _aliceEmail = "alice@example.com";
        
        private readonly Guid _bobId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
        // private readonly string _bobEmail = "bob@example.com";
        
        private readonly Guid _seededFeedbackId = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10");

        private async Task<(string token, Guid userId)> LoginSeededUserAsync(string email)
        {
            var password = ACCOUNT_PASSWORD;

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            
            if (!loginData.Success)
            {
                throw new Exception($"Failed to login seeded user {email}. Ensure DB is seeded and password is correct.");
            }

            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<Feedback> CreateFeedbackAsync(Guid candidateId, Guid coachId, Guid interviewRoomId)
        {
            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                CoachId = coachId,
                InterviewRoomId = interviewRoomId,
                Rating = 0,
                Comments = "",
                AIAnalysis = ""
            };

            // Using the DEBUG endpoint to seed data
            var response = await _api.PostAsync("/api/v1/feedbacks", feedback, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<Feedback>(response);
            
            return apiResponse.Data!;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetList_ReturnsSuccess_WhenCandidateIsAuthenticated()
        {
            // Arrange
            // Use seeded Alice (Candidate)
            var (candidateToken, _) = await LoginSeededUserAsync(_aliceEmail);

            // Act
            LogInfo("Getting feedback list as an authenticated candidate.");
            var response = await _api.GetAsync("/api/v1/feedbacks", jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data should not be null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenFeedbackAlreadyGiven()
        {
            // Arrange
            // Use seeded Alice and the seeded feedback which is already completed (Rating 5, Comments "Great answers...")
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
            var feedbackId = _seededFeedbackId; // Can use any ID as validation happens before DB check

            var updateDto = new UpdateFeedbackDto
            {
                Rating = 0, // Invalid rating
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
        
        /*[Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            // Use seeded Alice and Bob, but create a NEW feedback entry because the seeded one is already completed.
            var (candidateToken, candidateId) = await LoginSeededUserAsync(_aliceEmail);
            var coachId = _bobId;
            var feedback = await CreateFeedbackAsync(candidateId, coachId, Guid.NewGuid()); // Create fresh feedback
            
            var updateDto = new UpdateFeedbackDto
            {
                Rating = 5,
                Comments = "Excellent performance!"
            };

            // Act
            LogInfo("Updating a feedback with valid data.");
            var response = await _api.PutAsync($"/api/v1/feedbacks/{feedback.Id}", updateDto, jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Feedback update was successful");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var (candidateToken, candidateId) = await LoginSeededUserAsync(_aliceEmail);
            var coachId = _bobId;
            var feedback = await CreateFeedbackAsync(candidateId, coachId, Guid.NewGuid()); // Create fresh feedback
            
            // Act
            LogInfo("Attempting to update a feedback with a missing comment.");
            var responseNoComment = await _api.PutAsync($"/api/v1/feedbacks/{feedback.Id}", new UpdateFeedbackDto { Rating = 3, Comments = "" }, jwtToken: candidateToken, logBody: true);

            LogInfo("Attempting to update a feedback with a missing rating.");
            var responseNoRating = await _api.PutAsync($"/api/v1/feedbacks/{feedback.Id}", new UpdateFeedbackDto { Rating = 0, Comments = "Valid comment" }, jwtToken: candidateToken, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, responseNoComment.StatusCode, "Status code is 400 Bad Request for missing comment");
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, responseNoRating.StatusCode, "Status code is 400 Bad Request for missing rating");
        }*/
    }
}