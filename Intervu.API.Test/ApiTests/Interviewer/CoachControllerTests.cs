using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Intervu.Application.DTOs.Coach;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.User;
using Xunit.Abstractions;
using Intervu.Domain.Entities.Constants;

namespace Intervu.API.Test.ApiTests.Interviewer
{
    public class CoachControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CoachControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(Guid userId, string userToken, string coachSlug, Guid coachProfileId)> RegisterAndCreateCoachAsync(string fullNamePrefix = "Test Coach", string emailPrefix = "coach")
        {
            // 1. Register user as Interviewer
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

            LogInfo($"Registering new interviewer: {email}");
            var registerResponse = await _api.PostAsync("/api/v1/account/register", registerRequest, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, registerResponse.StatusCode, "Interviewer registration should succeed");

            // 2. Login as the newly registered interviewer to get their token and ID
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);
            await AssertHelper.AssertTrue(loginData.Success, "Interviewer login should succeed");
            var userId = loginData.Data!.User.Id;
            var userToken = loginData.Data.Token;

            // 3. Login as Admin to create the Coach Profile (since CreateCoachProfile is Admin-only)
            var adminLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminLoginData = await _api.LogDeserializeJson<LoginResponse>(adminLoginResponse);
            await AssertHelper.AssertTrue(adminLoginData.Success, "Admin login should succeed");
            var adminToken = adminLoginData.Data!.Token;

            // 4. Create Coach Profile using Admin token
            var createCoachDto = new CoachCreateDto
            {
                FullName = fullName,
                Email = email,
                Password = password, // Password is required by DTO but not used for profile creation directly
                Role = UserRole.Coach,
                UserStatus = UserStatus.Active,
                ExperienceYears = 5,
                CurrentAmount = 50,
                Status = CoachProfileStatus.Enable,
                CompanyIds = new List<Guid>(),
                SkillIds = new List<Guid>()
            };

            LogInfo($"Creating coach profile for user {userId} as Admin.");
            var createProfileResponse = await _api.PostAsync("/api/v1/coach-profile", createCoachDto, jwtToken: adminToken);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createProfileResponse.StatusCode, "Coach profile creation should succeed");

            // 5. Get the created coach profile to extract the actual profile ID
            var getProfileResponse = await _api.GetAsync($"/api/v1/coach-profile/{userId}", jwtToken: userToken);
            var coachProfile = await _api.LogDeserializeJson<CoachProfileDto>(getProfileResponse);
            await AssertHelper.AssertTrue(coachProfile.Success, "Fetching created coach profile should succeed");
            var coachProfileId = coachProfile.Data!.Id;

            return (userId, userToken, slug, coachProfileId);
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_ReturnsSuccessAndList()
        {
            // Arrange
            int page = 1;
            int pageSize = 10;
            var (userId, _, _, _) = await RegisterAndCreateCoachAsync();

            // Act
            LogInfo($"Getting all coaches with page {page} and pageSize {pageSize}.");
            var response = await _api.GetAsync($"/api/v1/coach?page={page}&pageSize={pageSize}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");

            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data object should not be null");
            await AssertHelper.AssertNotEmpty(apiResponse.Data!.Items, "Items list should not be empty");

            // Verify the newly created coach exists
            var newCoach = apiResponse.Data.Items.FirstOrDefault(c => c.Id == userId);
            await AssertHelper.AssertNotNull(newCoach, $"Newly created coach {userId} should exist in the list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_WithFilters_ReturnsFilteredList()
        {
            // Arrange
            // Create a coach that matches specific filters
            var (coach1Id, _, _, _) = await RegisterAndCreateCoachAsync("FilterCoach1", "filtercoach1");
            // Create another coach that should not match
            var (coach2Id, _, _, _) = await RegisterAndCreateCoachAsync("FilterCoach2", "filtercoach2");

            string searchTerm = "FilterCoach1";
            int minExperienceYears = 4; // Coach1 has 5 years experience
            int maxExperienceYears = 6;
            int minPrice = 40;
            int maxPrice = 60;

            // Act
            LogInfo($"Filtering coaches with searchTerm='{searchTerm}', experience {minExperienceYears}-{maxExperienceYears}, price {minPrice}-{maxPrice}.");
            var response = await _api.GetAsync($"/api/v1/coach?searchTerm={searchTerm}&minExperienceYears={minExperienceYears}&maxExperienceYears={maxExperienceYears}&minPrice={minPrice}&maxPrice={maxPrice}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data object should not be null");

            // Check if coach1 is in the returned list
            var coach1Found = apiResponse.Data.Items.Any(c => c.Id == coach1Id);
            await AssertHelper.AssertTrue(coach1Found, "Coach1 should match the filters and be returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_Pagination_RespectsPageSize()
        {
            // Arrange
            // Create more coaches than the page size to test pagination
            for (int i = 0; i < 3; i++)
            {
                await RegisterAndCreateCoachAsync($"PaginationCoach{i}", $"paginationcoach{i}");
            }

            int page = 1;
            int pageSize = 2;

            // Act
            LogInfo($"Getting coaches with page {page} and pageSize {pageSize}.");
            var response = await _api.GetAsync($"/api/v1/coach?page={page}&pageSize={pageSize}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data object should not be null");
            await AssertHelper.AssertEqual(pageSize, apiResponse.Data.Items.Count, $"Page size should be {pageSize}");
            await AssertHelper.AssertEqual(page, apiResponse.Data.CurrentPage, "Returned page number should match request");
            await AssertHelper.AssertEqual(pageSize, apiResponse.Data.PageSize, "Returned page size should match request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Coach")]
        public async Task GetAllCoach_WithSkillIds_ReturnsFilteredList()
        {
            // Arrange
            var skillId1 = Guid.Parse("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1"); // Dummy skill ID
            var skillId2 = Guid.Parse("02020202-0202-4202-8202-020202020202"); // Another dummy skill ID

            // Re-create coach1 with a specific skill
            var email1 = $"skillcoach1_{Guid.NewGuid()}@example.com";
            var password = CANDIDATE_PASSWORD;
            var fullName1 = $"SkillCoach1 {Guid.NewGuid().ToString().Substring(0, 4)}";
            var slug1 = $"coach-slug-{Guid.NewGuid().ToString().Substring(0, 8)}";

            var registerRequest1 = new RegisterRequest
            {
                Email = email1,
                Password = password,
                FullName = fullName1,
                Role = "Coach",
                SlugProfileUrl = slug1
            };
            await _api.PostAsync("/api/v1/account/register", registerRequest1);
            var loginResponse1 = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email1, Password = password });
            var loginData1 = await _api.LogDeserializeJson<LoginResponse>(loginResponse1);
            var userId1 = loginData1.Data!.User.Id;
            var userToken1 = loginData1.Data.Token;

            var adminLoginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = ADMIN_EMAIL, Password = DEFAULT_PASSWORD });
            var adminLoginData = await _api.LogDeserializeJson<LoginResponse>(adminLoginResponse);
            var adminToken = adminLoginData.Data!.Token;

            var createCoachDto1 = new CoachCreateDto
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
                SkillIds = new List<Guid> { skillId1 } // Assign skillId1
            };
            await _api.PostAsync("/api/v1/coach-profile", createCoachDto1, jwtToken: adminToken);

            // Act - Filter by skillId1
            LogInfo($"Filtering coaches with skillIds='{skillId1}'.");
            var response = await _api.GetAsync($"/api/v1/coach?skillIds={skillId1}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<CoachProfileDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "Request was successful");
            await AssertHelper.AssertNotNull(apiResponse.Data, "Data object should not be null");

            var coach1Found = apiResponse.Data.Items.Any(c => c.Id == userId1);
            await AssertHelper.AssertTrue(coach1Found, "Coach1 with specific skill should be returned");
        }
    }
}