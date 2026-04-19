using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AiController
{
    // IC-53
    public class ViewAICVEvaluationTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewAICVEvaluationTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginCandidateAsync()
        {
            var loginRequest = new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD };
            var response = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<string> LoginCoachAsync()
        {
            var loginRequest = new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD };
            var response = await _api.PostAsync("/api/v1/account/login", loginRequest);
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        private async Task<HttpResponseMessage> EvaluateCvAsync(string jwtToken = "")
        {
            var dummyPdf = Encoding.UTF8.GetBytes("%PDF-1.4\n1 0 obj\n<<>>\nendobj\ntrailer\n<<>>\n%%EOF");
            return await _api.PostMultipartAsync(
                "/api/v1/candidate-profile/evaluate-cv",
                dummyPdf,
                "cv.pdf",
                "application/pdf",
                "file",
                jwtToken: jwtToken,
                logBody: true);
        }

        [Fact(Skip="Cannot provide valid CV for testing")]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task ViewAICVEvaluation_ReturnsSuccess_WhenValidCVProvided()
        {
            // Arrange
            var token = await LoginCandidateAsync();

            // Act
            var response = await EvaluateCvAsync(token);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task ViewAICVEvaluation_ReturnsUnauthorized_WhenNoToken()
        {
            // Act
            var response = await EvaluateCvAsync();

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task ViewAICVEvaluation_ReturnsForbidden_WhenRoleIsNotCandidate()
        {
            // Arrange
            var token = await LoginCoachAsync();

            // Act
            var response = await EvaluateCvAsync(token);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Coach role returns 403 Forbidden");
        }
    }
}
