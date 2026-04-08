using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Candidate;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CandidateProfileController
{
    public class ViewCandidatePublicProfileTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewCandidatePublicProfileTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> RegisterAndGetSlugAsync()
        {
            var email = $"candidate_{Guid.NewGuid()}@example.com";
            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = CANDIDATE_PASSWORD,
                FullName = "Test Candidate",
                SlugProfileUrl = $"slug-{Guid.NewGuid()}"
            }, logBody: true);

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = CANDIDATE_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);
            return loginData.Data!.User.SlugProfileUrl;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CandidateProfile")]
        public async Task GetProfileBySlug_ReturnsSuccess_WhenSlugExists()
        {
            var slug = await RegisterAndGetSlugAsync();

            var response = await _api.GetAsync($"/api/v1/candidate-profile/{slug}/profile", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<CandidateProfileDto>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Profile data returned");
            await AssertHelper.AssertEqual(slug, apiResponse.Data!.User.SlugProfileUrl, "Slug matches");
        }
    }
}
