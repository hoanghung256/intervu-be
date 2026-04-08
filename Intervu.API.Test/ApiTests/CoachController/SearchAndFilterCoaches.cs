using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities.Constants;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.CoachController
{
    public class SearchAndFilterCoachesTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SearchAndFilterCoachesTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid userId, string userToken, string coachSlug, Guid coachProfileId)> RegisterAndCreateCoachAsync(string fullNamePrefix = "Test Coach", string emailPrefix = "coach")
        {
            var email = $"{emailPrefix}_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            var fullName = $"{fullNamePrefix} {Guid.NewGuid().ToString().Substring(0, 4)}";
            var slug = $"coach-slug-{Guid.NewGuid().ToString().Substring(0, 8)}";

            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = fullName,
                Role = UserRole.Coach.ToString(),
                SlugProfileUrl = slug
            };

            var registerResponse = await _api.PostAsync("/api/v1/account/register", registerRequest, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, registerResponse.StatusCode, "Interviewer registration should succeed");

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);
            var userId = loginData.Data!.User.Id;
            var userToken = loginData.Data.Token;

            var adminLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminLoginData = await _api.LogDeserializeJson<LoginResponse>(adminLoginResponse);
            var adminToken = adminLoginData.Data!.Token;

            var createCoachDto = new CoachCreateDto
            {
                FullName = fullName,
                Email = email,
                Password = password,
                Role = UserRole.Coach,
                UserStatus = UserStatus.Active,
                ExperienceYears = 5,
                CurrentAmount = 50,
                Status = CoachProfileStatus.Enable,
                CompanyIds = new List<Guid>(),
                SkillIds = new List<Guid>()
            };

            var createProfileResponse = await _api.PostAsync("/api/v1/coach-profile", createCoachDto, jwtToken: adminToken);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createProfileResponse.StatusCode, "Coach profile creation should succeed");

            var getProfileResponse = await _api.GetAsync($"/api/v1/coach-profile/{userId}", jwtToken: userToken);
            var coachProfile = await _api.LogDeserializeJson<CoachProfileDto>(getProfileResponse);
            var coachProfileId = coachProfile.Data!.Id;

            return (userId, userToken, slug, coachProfileId);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_ReturnsSuccessAndList()
        {
            int page = 1;
            int pageSize = 10;
            var (userId, _, _, _) = await RegisterAndCreateCoachAsync();

            var response = await _api.GetAsync($"/api/v1/coach?page={page}&pageSize={pageSize}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data object should not be null");
            await AssertHelper.AssertNotEmpty(apiResponse.Data!.Items, "Items list should not be empty");
            var newCoach = apiResponse.Data.Items.FirstOrDefault(c => c.Id == userId);
            await AssertHelper.AssertNotNull(newCoach, $"Newly created coach {userId} should exist in the list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_WithFilters_ReturnsFilteredList()
        {
            var (coach1Id, _, _, _) = await RegisterAndCreateCoachAsync("FilterCoach1", "filtercoach1");
            await RegisterAndCreateCoachAsync("FilterCoach2", "filtercoach2");

            var response = await _api.GetAsync("/api/v1/coach?searchTerm=FilterCoach1&minExperienceYears=4&maxExperienceYears=6&minPrice=40&maxPrice=60", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            var coach1Found = apiResponse.Data!.Items.Any(c => c.Id == coach1Id);
            await AssertHelper.AssertTrue(coach1Found, "Coach1 should match the filters and be returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_Pagination_RespectsPageSize()
        {
            for (int i = 0; i < 3; i++)
            {
                await RegisterAndCreateCoachAsync($"PaginationCoach{i}", $"paginationcoach{i}");
            }

            var response = await _api.GetAsync("/api/v1/coach?page=1&pageSize=2", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertEqual(2, apiResponse.Data!.Items.Count, "Page size should be 2");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_WithSkillIds_ReturnsFilteredList()
        {
            var skillId1 = Guid.Parse("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1");
            var email1 = $"skillcoach1_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            var fullName1 = $"SkillCoach1 {Guid.NewGuid().ToString().Substring(0, 4)}";

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email1,
                Password = password,
                FullName = fullName1,
                Role = "Coach",
                SlugProfileUrl = $"coach-slug-{Guid.NewGuid().ToString().Substring(0, 8)}"
            });

            var loginResponse1 = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email1, Password = password });
            var loginData1 = await _api.LogDeserializeJson<LoginResponse>(loginResponse1);
            var userId1 = loginData1.Data!.User.Id;

            var adminLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminLoginData = await _api.LogDeserializeJson<LoginResponse>(adminLoginResponse);

            await _api.PostAsync("/api/v1/coach-profile", new CoachCreateDto
            {
                FullName = fullName1,
                Email = email1,
                Password = password,
                Role = UserRole.Coach,
                UserStatus = UserStatus.Active,
                ExperienceYears = 5,
                CurrentAmount = 50,
                Status = CoachProfileStatus.Enable,
                CompanyIds = new List<Guid>(),
                SkillIds = new List<Guid> { skillId1 }
            }, jwtToken: adminLoginData.Data!.Token);

            var response = await _api.GetAsync($"/api/v1/coach?skillIds={skillId1}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Data!.Items.Any(c => c.Id == userId1), "Coach1 with specific skill should be returned");
        }
    }
}
