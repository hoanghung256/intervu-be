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

        [Fact]
        public async Task Handle_ValidCandidateCvFile_UploadsSuccessfully()
        {
            var email = $"candidate_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Candidate", Role = "Candidate" });
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var tokenData = await _api.LogDeserializeJson<LoginResponse>(login);
            var userId = tokenData.Data!.User.Id;

            var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-cv/{userId}", Encoding.UTF8.GetBytes("dummy cv"), "cv.pdf", "application/pdf", "file", jwtToken: tokenData.Data.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        public async Task Handle_NonExistentUser_ReturnsNotFound()
        {
            var email = $"candidate_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest { Email = email, Password = CANDIDATE_PASSWORD, FullName = "Candidate", Role = "Candidate" });
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var tokenData = await _api.LogDeserializeJson<LoginResponse>(login);

            var response = await _api.PostMultipartAsync($"/api/v1/userprofile/upload-cv/{Guid.NewGuid()}", Encoding.UTF8.GetBytes("dummy cv"), "cv.pdf", "application/pdf", "file", jwtToken: tokenData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 NotFound");
        }
    }
}
