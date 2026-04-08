using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CoachProfileController
{
    public class UpdateCoachProfileTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly string _bobEmail = "bob@example.com";

        public UpdateCoachProfileTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> LoginSeededUserAsync(string email)
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachProfile_ReturnsSuccess_WhenDataIsValid()
        {
            var (token, userId) = await LoginSeededUserAsync(_bobEmail);

            var updateDto = new CoachUpdateDto
            {
                FullName = "Updated Bob",
                Email = "updated@example.com",
                Bio = "Updated Bio for testing purposes.",
                ExperienceYears = 9,
                PortfolioUrl = "https://updated-portfolio.example.com",
                CurrentAmount = 100
            };

            var response = await _api.PutAsync($"/api/v1/coach-profile/{userId}", updateDto, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Update was successful");
            await AssertHelper.AssertEqual("Profile updated successfully!", apiResponse.Message, "Message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachProfile")]
        public async Task UpdateCoachProfile_ReturnsUnauthorized_WhenNoToken()
        {
            var (_, userId) = await LoginSeededUserAsync(_bobEmail);
            var updateDto = new CoachUpdateDto
            {
                FullName = "Unauthorized Update",
                Email = "unauthorized@example.com",
                Bio = "Unauthorized update payload",
                ExperienceYears = 3,
                PortfolioUrl = "https://unauthorized.example.com",
                CurrentAmount = 1
            };

            var response = await _api.PutAsync($"/api/v1/coach-profile/{userId}", updateDto, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }
    }
}
