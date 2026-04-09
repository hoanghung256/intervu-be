using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Comment;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CommentController
{
    public class ReportCommentTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _q1Id = Guid.Parse("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a");

        public ReportCommentTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid commentId)> SetupCommentAsync()
        {
            var email = $"comment_user_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Comment Tester", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            var token = loginData.Data!.Token;

            var addResponse = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments", new CreateCommentRequest { Content = "Test comment for reporting." }, jwtToken: token);
            var addResult = await _api.LogDeserializeJson<Guid>(addResponse);
            return (token, addResult.Data);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Comment")]
        public async Task ReportComment_Success_ReturnsOk()
        {
            var (token, commentId) = await SetupCommentAsync();

            var response = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}/report", new
            {
                Reason = "Inappropriate content",
                Description = "This comment contains offensive language."
            }, jwtToken: token, logBody: true);

            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Report status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Report request successful");
            await AssertHelper.AssertEqual("Comment reported successfully", payload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Comment")]
        public async Task ReportComment_Unauthorized_ReturnsUnauthorized()
        {
            var (_, commentId) = await SetupCommentAsync();

            var response = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}/report", new
            {
                Reason = "Spam",
                Description = "Spamming the same message."
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Comment")]
        public async Task ReportComment_NonExistentComment_ReturnsNotFound()
        {
            var email = $"reporter_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Reporter", Role = "Candidate" });
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(loginResponse)).Data!.Token;

            var response = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments/{Guid.NewGuid()}/report", new
            {
                Reason = "Harassment",
                Description = "Targeting a specific user."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Reporting non-existent comment should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Comment")]
        public async Task ReportComment_MissingReason_ReturnsBadRequest()
        {
            var (token, commentId) = await SetupCommentAsync();

            var response = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}/report", new
            {
                Reason = "", // Empty reason
                Description = "Reason is missing."
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing reason should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Comment")]
        public async Task ReportComment_DuplicateReportBySameUser_ReturnsConflict()
        {
            var (token, commentId) = await SetupCommentAsync();

            // First report
            await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}/report", new
            {
                Reason = "Spam",
                Description = "First report."
            }, jwtToken: token, logBody: true);

            // Second report by same user
            var response = await _api.PostAsync($"/api/v1/questions/{_q1Id}/comments/{commentId}/report", new
            {
                Reason = "Spam",
                Description = "Duplicate report."
            }, jwtToken: token, logBody: true);

            // Assuming a user can only report a comment once
            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, response.StatusCode, "Duplicate report by same user should return 409 Conflict");
        }
    }
}
