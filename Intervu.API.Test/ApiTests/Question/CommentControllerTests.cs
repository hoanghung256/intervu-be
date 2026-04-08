using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Comment;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;
using Intervu.Application.DTOs.Common;

namespace Intervu.API.Test.ApiTests.Question
{
    public class CommentControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CommentControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginUserAsync()
        {
            var email = $"comment_user_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = "Comment Tester",
                Role = "Candidate"
            });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        // Seeded Question ID from IntervuPostgreDbContext
        private readonly Guid _q1Id = Guid.Parse("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a");

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Comment")]
        public async Task Comment_Lifecycle_ReturnsSuccess()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginUserAsync();

            // 1. Add Comment
            var createRequest = new CreateCommentRequest
            {
                Content = "This is a test comment for the lifecycle test."
            };
            LogInfo($"Adding comment to question {_q1Id}.");
            var addResponse = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments", createRequest, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, addResponse.StatusCode, "Add status code is 200 OK");
            var addResult = await _api.LogDeserializeJson<Guid>(addResponse);
            var commentId = addResult.Data;

            // 2. Get Comments
            LogInfo($"Getting comments for question {_q1Id}.");
            var getListResponse = await _api.GetAsync($"/api/v1/questions/{_q1Id}/comments", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getListResponse.StatusCode, "Get list status code is 200 OK");
            var listData = await _api.LogDeserializeJson<PagedResult<CommentDto>>(getListResponse);
            await AssertHelper.AssertTrue(listData.Data!.Items.Any(c => c.Id == commentId), "Added comment is in the list");

            // 3. Like Comment
            LogInfo($"Liking comment {commentId}.");
            var likeResponse = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}/like", new { }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, likeResponse.StatusCode, "Like status code is 200 OK");

            // 4. Update Comment
            var updateRequest = new UpdateCommentRequest
            {
                Content = "This is an updated test comment."
            };
            LogInfo($"Updating comment {commentId}.");
            var updateResponse = await _api.PutAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}", updateRequest, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");

            // 5. Delete Comment
            LogInfo($"Deleting comment {commentId}.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Comment")]
        public async Task GetComments_ReturnsSuccess_EvenWithoutToken()
        {
            // Act
            LogInfo($"Getting comments for question {_q1Id} anonymously.");
            var response = await _api.GetAsync($"/api/v1/questions/{_q1Id}/comments", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for anonymous users");
        }
    }
}