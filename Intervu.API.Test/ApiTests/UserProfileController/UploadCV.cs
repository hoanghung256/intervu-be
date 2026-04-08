using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    public class UploadCVTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public UploadCVTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid userId, string token)> RegisterAndLoginUserAsync()
        {
            var email = $"user_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD,
                FullName = "Test User",
                Role = "Candidate"
            });

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);

            return (loginData.Data!.User.Id, loginData.Data.Token);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task UploadCV_ReturnsSuccess_WhenFileIsValid()
        {
            var (userId, token) = await RegisterAndLoginUserAsync();
            var fileContent = Encoding.UTF8.GetBytes("This is a dummy CV file for testing.");

            LogInfo($"Uploading CV for user {userId}.");
            var response = await _api.PostMultipartAsync(
                $"/api/v1/userprofile/upload-cv/{userId}",
                fileContent,
                "cv.pdf",
                "application/pdf",
                "file",
                jwtToken: token,
                logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<string>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "CV upload was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "CV URL is returned");
        }
    }
}
