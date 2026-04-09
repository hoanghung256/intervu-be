using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Feedback;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.FeedbacksController
{
    public class ProvideFeedbackAfterInterviewTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _feedbackUpdatePendingId = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c11");

        public ProvideFeedbackAfterInterviewTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsSuccess_WhenDataIsValid()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PutAsync($"/api/v1/feedbacks/{_feedbackUpdatePendingId}", new UpdateFeedbackDto
            {
                Rating = 5,
                Comments = "Excellent performance! The coach was very helpful."
            }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.PutAsync($"/api/v1/feedbacks/{_feedbackUpdatePendingId}", new UpdateFeedbackDto
            {
                Rating = 5,
                Comments = "Unauthorized request"
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenCommentIsNull()
        {
            // Arrange – null Comments must be rejected by the IsNullOrEmpty guard in the controller
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            // Act
            var response = await _api.PutAsync($"/api/v1/feedbacks/{_feedbackUpdatePendingId}", new UpdateFeedbackDto
            {
                Rating = 5,
                Comments = null!
            }, jwtToken: loginData.Data!.Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request when Comments is null");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenCommentsExceedMaxLength()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var longComment = new string('A', 2001); // Assuming 2000 is the limit
            var response = await _api.PutAsync($"/api/v1/feedbacks/{_feedbackUpdatePendingId}", new UpdateFeedbackDto { Rating = 5, Comments = longComment }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Too long comment returns 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenRatingIsTooLow()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.PutAsync($"/api/v1/feedbacks/{_feedbackUpdatePendingId}", new UpdateFeedbackDto { Rating = -1, Comments = "Invalid rating" }, jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Negative rating returns 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Feedbacks")]
        public async Task UpdateFeedback_ReturnsNotFound_WhenFeedbackIdDoesNotExist()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var nonExistentId = Guid.NewGuid();

            var response = await _api.PutAsync($"/api/v1/feedbacks/{nonExistentId}", new UpdateFeedbackDto { Rating = 5, Comments = "Valid comment" }, jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent feedback ID returns 404 Not Found");
        }
    }
}
