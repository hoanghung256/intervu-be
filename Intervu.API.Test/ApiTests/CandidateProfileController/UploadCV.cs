using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CandidateProfileController
{
    public class UploadCVTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public UploadCVTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        private async Task<(string token, Guid userId)> RegisterAndLoginCandidateAsync()
        {
            var email = $"candidate_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Candidate", Role = "Candidate" });
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var tokenData = await _api.LogDeserializeJson<LoginResponse>(login);
            return (tokenData.Data!.Token, tokenData.Data.User.Id);
        }

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task Handle_ValidCandidateCvFile_UploadsSuccessfully()
        {
            // Arrange
            var (token, userId) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-cv/{userId}", Encoding.UTF8.GetBytes("dummy cv"), "cv.pdf", "application/pdf", "file", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task Handle_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-cv/{Guid.NewGuid()}", Encoding.UTF8.GetBytes("dummy cv"), "cv.pdf", "application/pdf", "file", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 NotFound");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task Handle_EmptyGuidUser_ReturnsNotFound()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-cv/{Guid.Empty}", Encoding.UTF8.GetBytes("dummy cv"), "cv.pdf", "application/pdf", "file", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Empty GUID returns 404 NotFound");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task Handle_UploadCV_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var (_, userId) = await RegisterAndLoginCandidateAsync();

            // Act
            var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-cv/{userId}", Encoding.UTF8.GetBytes("dummy cv"), "cv.pdf", "application/pdf", "file", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token upload returns 401 Unauthorized");
        }
    }
}
