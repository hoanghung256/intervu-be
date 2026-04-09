using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Feedback;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.FeedbacksController
{
    public class ViewFeedbackTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _seededFeedbackId = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10");
        private readonly Guid _feedbackUpdatePendingId = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c11");

        public ViewFeedbackTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetList_ReturnsSuccess_WhenCandidateIsAuthenticated()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync("/api/v1/feedbacks", jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetList_ReturnsSuccess_WithCustomPagination()
        {
            // Arrange
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // Act – explicit page and pageSize query params
            var response = await _api.GetAsync("/api/v1/feedbacks?page=1&pageSize=5", jwtToken: loginData.Data!.Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK with custom pagination");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetList_ReturnsUnauthorized_WhenNoToken()
        {
            // Act – no Authorization header
            var response = await _api.GetAsync("/api/v1/feedbacks", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task GetList_ReturnsForbidden_WhenCalledByCoach()
        {
            // Arrange – bob is a Coach; the GetList endpoint requires Candidate policy
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // Act
            var response = await _api.GetAsync("/api/v1/feedbacks", jwtToken: loginData.Data!.Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden for Coach role");
        }


        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenFeedbackAlreadyGiven()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PutAsync($"/api/v1/feedbacks/{_seededFeedbackId}", new UpdateFeedbackDto { Rating = 5, Comments = "Trying to update again" }, jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenRatingIsZero()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PutAsync($"/api/v1/feedbacks/{_feedbackUpdatePendingId}", new UpdateFeedbackDto { Rating = 0, Comments = "Valid comment" }, jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenCommentIsEmpty()
        {
            // Arrange
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // Act – empty Comments string hits the IsNullOrEmpty guard in the controller
            var response = await _api.PutAsync($"/api/v1/feedbacks/{_feedbackUpdatePendingId}", new UpdateFeedbackDto { Rating = 5, Comments = "" }, jwtToken: loginData.Data!.Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request when comment is empty");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsForbidden_WhenCalledByCoach()
        {
            // Arrange – bob is a Coach; the UpdateFeedback endpoint requires Candidate policy
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // Act
            var response = await _api.PutAsync($"/api/v1/feedbacks/{_feedbackUpdatePendingId}", new UpdateFeedbackDto { Rating = 5, Comments = "Coach trying to submit feedback" }, jwtToken: loginData.Data!.Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Status code is 403 Forbidden for Coach role");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsServerError_WhenFeedbackIdDoesNotExist()
        {
            // Arrange – use a random Guid that has no matching Feedback row.
            // NOTE: This test documents a known NullReferenceException bug in FeedbacksController.UpdateFeedback:
            // after the `feedback?.Comments` null-guard passes, the code accesses `feedback.Rating` on a null reference.
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _api.PutAsync($"/api/v1/feedbacks/{nonExistentId}", new UpdateFeedbackDto { Rating = 5, Comments = "Valid comment" }, jwtToken: loginData.Data!.Token, logBody: true);

            // Assert – controller throws NullReferenceException; framework returns 500
            await AssertHelper.AssertEqual(HttpStatusCode.InternalServerError, response.StatusCode, "Status code is 500 when feedback ID does not exist (bug: no null guard before property assignment)");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsNotFound_WhenFeedbackDoesNotExist()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var nonExistentId = Guid.NewGuid();

            var response = await _api.PutAsync($"/api/v1/feedbacks/{nonExistentId}", new UpdateFeedbackDto { Rating = 5, Comments = "Valid comment" }, jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent feedback ID returns 404 Not Found");
        }
    }
}
