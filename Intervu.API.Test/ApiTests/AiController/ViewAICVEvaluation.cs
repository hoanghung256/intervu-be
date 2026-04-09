using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AiController
{
    public class ViewAICVEvaluationTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _seededCvId = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"); // Placeholder for a valid CV ID

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

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task ViewAICVEvaluation_ReturnsSuccess_WhenValidCVProvided()
        {
            // Arrange
            var token = await LoginCandidateAsync();

            // Act
            var response = await _api.GetAsync($"/api/v1/ai/cv-evaluation/{_seededCvId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            // Assuming a DTO for CV evaluation exists, e.g., CvEvaluationDto
            // var apiResponse = await _api.LogDeserializeJson<CvEvaluationDto>(response);
            // await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            // await AssertHelper.AssertNotNull(apiResponse.Data, "CV Evaluation data is not null");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task ViewAICVEvaluation_ReturnsUnauthorized_WhenNoToken()
        {
            // Act
            var response = await _api.GetAsync($"/api/v1/ai/cv-evaluation/{_seededCvId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task ViewAICVEvaluation_ReturnsNotFound_WhenCVDoesNotExist()
        {
            // Arrange
            var token = await LoginCandidateAsync();
            var nonExistentCvId = Guid.NewGuid();

            // Act
            var response = await _api.GetAsync($"/api/v1/ai/cv-evaluation/{nonExistentCvId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent CV ID returns 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task ViewAICVEvaluation_ReturnsForbidden_WhenUserDoesNotOwnCV()
        {
            // Arrange
            // Assuming there's another candidate whose CV we try to access
            var token = await LoginCandidateAsync();
            var otherUserCvId = Guid.Parse("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"); // Placeholder for another user's CV ID

            // Act
            var response = await _api.GetAsync($"/api/v1/ai/cv-evaluation/{otherUserCvId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Accessing another user's CV returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "AI")]
        public async Task ViewAICVEvaluation_ReturnsBadRequest_WhenInvalidCVIdFormat()
        {
            // Arrange
            var token = await LoginCandidateAsync();
            var invalidCvId = "not-a-guid";

            // Act
            var response = await _api.GetAsync($"/api/v1/ai/cv-evaluation/{invalidCvId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid CV ID format returns 400 Bad Request");
        }
    }
}
