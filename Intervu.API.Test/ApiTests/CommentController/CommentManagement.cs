using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Comment;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CommentController
{
    // TODO: split to multiple test classes - AddComment, UpdateComment, DeleteComment, LikeComment, GetComments
    public class CommentManagementTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _q1Id = Guid.Parse("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a");

        public CommentManagementTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> RegisterAndLoginUserAsync()
        {
            var email = $"comment_user_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Comment Tester", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Comment")]
        public async Task Comment_Lifecycle_ReturnsSuccess()
        {
            var token = await RegisterAndLoginUserAsync();
            var addResponse = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments", new CreateCommentRequest { Content = "This is a test comment for the lifecycle test." }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, addResponse.StatusCode, "Add status code is 200 OK");
            var addResult = await _api.LogDeserializeJson<Guid>(addResponse);
            var commentId = addResult.Data;

            var getListResponse = await _api.GetAsync($"/api/v1/questions/{_q1Id}/comments", logBody: true);
            var listData = await _api.LogDeserializeJson<PagedResult<CommentDto>>(getListResponse);
            await AssertHelper.AssertTrue(listData.Data!.Items.Any(c => c.Id == commentId), "Added comment is in the list");

            await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}/like", new { }, jwtToken: token, logBody: true);
            var updateResponse = await _api.PutAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}", new UpdateCommentRequest { Content = "This is an updated test comment." }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }
    }
}
