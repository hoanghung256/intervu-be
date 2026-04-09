using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Intervu.Application.DTOs.CoachInterviewService;
using Intervu.Application.DTOs.User;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.Interviewer
{
    public class CoachInterviewServiceControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public CoachInterviewServiceControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginCoachAsync(string emailPrefix = "coach_service")
        {
            var email = $"{emailPrefix}_{Guid.NewGuid()}@example.com";
            var password = DEFAULT_PASSWORD;
            var fullName = "Test Coach Service";

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = fullName,
                Role = "Coach"
            });

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        private async Task<(string token, Guid userId)> RegisterAndLoginCandidateAsync(string emailPrefix = "candidate_service")
        {
            var email = $"{emailPrefix}_{Guid.NewGuid()}@example.com";
            var password = DEFAULT_PASSWORD;
            var fullName = "Test Candidate Service";

            await _api.PostAsync("/api/v1/account/register", new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = fullName,
                Role = "Candidate"
            });

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        // Seeded Interview Type ID from IntervuPostgreDbContext
        private readonly Guid _cvInterviewTypeId = Guid.Parse("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa");
        private readonly Guid _nonExistentInterviewTypeId = Guid.NewGuid();

        // ===== [N] Normal / Happy Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task CreateCoachInterviewService_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("create_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };

            // Act
            var response = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Create status code is 200 OK");
            var result = await _api.LogDeserializeJson<CoachInterviewServiceDto>(response);
            await AssertHelper.AssertTrue(result.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(result.Data, "Service data is not null");
            await AssertHelper.AssertEqual(createDto.Price, result.Data!.Price, "Price matches");
            await AssertHelper.AssertEqual(createDto.DurationMinutes, result.Data.DurationMinutes, "Duration matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task GetMyCoachInterviewServices_ReturnsSuccess_WhenCoachHasServices()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("get_mine_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1000,
                DurationMinutes = 30
            };
            await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token);

            // Act
            var response = await _api.GetAsync("/api/v1/coach-interview-services/mine", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Get mine status code is 200 OK");
            var result = await _api.LogDeserializeJson<List<CoachInterviewServiceDto>>(response);
            await AssertHelper.AssertTrue(result.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(result.Data, "Service list is not null");
            await AssertHelper.AssertNotEmpty(result.Data, "Service list is not empty");
            await AssertHelper.AssertTrue(result.Data!.Any(s => s.Price == createDto.Price), "Created service found in list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task GetCoachInterviewServicesByCoachId_ReturnsSuccess_WhenCoachHasServices()
        {
            // Arrange
            var (token, coachId) = await RegisterAndLoginCoachAsync("get_by_id_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 2000,
                DurationMinutes = 60
            };
            await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token);

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-interview-services/coach/{coachId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Get by coach ID status code is 200 OK");
            var result = await _api.LogDeserializeJson<List<CoachInterviewServiceDto>>(response);
            await AssertHelper.AssertTrue(result.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(result.Data, "Service list is not null");
            await AssertHelper.AssertNotEmpty(result.Data, "Service list is not empty");
            await AssertHelper.AssertTrue(result.Data!.Any(s => s.Price == createDto.Price), "Created service found in list");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task UpdateCoachInterviewService_ReturnsSuccess_WhenDataIsValid()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("update_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };
            var createResponse = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token);
            var createResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(createResponse);
            var serviceId = createResult.Data!.Id;

            var updateDto = new UpdateCoachInterviewServiceDto
            {
                Price = 1800,
                DurationMinutes = 50
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-interview-services/{serviceId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Update status code is 200 OK");
            var result = await _api.LogDeserializeJson<CoachInterviewServiceDto>(response);
            await AssertHelper.AssertTrue(result.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(result.Data, "Service data is not null");
            await AssertHelper.AssertEqual(updateDto.Price, result.Data!.Price, "Price updated");
            await AssertHelper.AssertEqual(updateDto.DurationMinutes, result.Data.DurationMinutes, "Duration updated");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteCoachInterviewService_ReturnsSuccess_WhenServiceExists()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("delete_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };
            var createResponse = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token);
            var createResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(createResponse);
            var serviceId = createResult.Data!.Id;

            // Act
            var response = await _api.DeleteAsync($"/api/v1/coach-interview-services/{serviceId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Delete status code is 200 OK");

            // Verify deletion
            var getResponse = await _api.GetAsync("/api/v1/coach-interview-services/mine", jwtToken: token);
            var getResult = await _api.LogDeserializeJson<List<CoachInterviewServiceDto>>(getResponse);
            await AssertHelper.AssertFalse(getResult.Data!.Any(s => s.Id == serviceId), "Deleted service should not be found");
        }

        // ===== [B] Boundary Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task CreateCoachInterviewService_ReturnsBadRequest_WhenPriceIsZero()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("price_zero_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 0,
                DurationMinutes = 30
            };

            // Act
            var response = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Price zero returns 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task CreateCoachInterviewService_ReturnsBadRequest_WhenDurationIsNegative()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("duration_negative_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1000,
                DurationMinutes = -1
            };

            // Act
            var response = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Negative duration returns 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task UpdateCoachInterviewService_ReturnsBadRequest_WhenPriceIsNegative()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("update_price_negative_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1000,
                DurationMinutes = 30
            };
            var createResponse = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token);
            var createResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(createResponse);
            var serviceId = createResult.Data!.Id;

            var updateDto = new UpdateCoachInterviewServiceDto
            {
                Price = -100,
                DurationMinutes = 30
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-interview-services/{serviceId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Negative price update returns 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task GetMyCoachInterviewServices_ReturnsEmptyList_WhenCoachHasNoServices()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("no_service_coach");

            // Act
            var response = await _api.GetAsync("/api/v1/coach-interview-services/mine", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var result = await _api.LogDeserializeJson<List<CoachInterviewServiceDto>>(response);
            await AssertHelper.AssertTrue(result.Success, "API response indicates success");
            await AssertHelper.AssertNotNull(result.Data, "Service list is not null");
            await AssertHelper.AssertEmpty(result.Data, "Service list should be empty for new coach");
        }

        // ===== [A] Abnormal / Error Path Tests =====

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task CreateCoachInterviewService_ReturnsUnauthorized_WhenNoToken()
        {
            // Arrange
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };

            // Act
            var response = await _api.PostAsync("/api/v1/coach-interview-services", createDto, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "No token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task CreateCoachInterviewService_ReturnsForbidden_WhenUserIsNotCoach()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCandidateAsync();
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };

            // Act
            var response = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Non-coach returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task CreateCoachInterviewService_ReturnsBadRequest_WhenInvalidInterviewTypeId()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("invalid_type_coach");
            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _nonExistentInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };

            // Act
            var response = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid interview type ID returns 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task UpdateCoachInterviewService_ReturnsNotFound_WhenServiceDoesNotExist()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("update_nonexistent_coach");
            var nonExistentServiceId = Guid.NewGuid();
            var updateDto = new UpdateCoachInterviewServiceDto
            {
                Price = 1800,
                DurationMinutes = 50
            };

            // Act
            var response = await _api.PutAsync($"/api/v1/coach-interview-services/{nonExistentServiceId}", updateDto, jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent service ID returns 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task UpdateCoachInterviewService_ReturnsForbidden_WhenServiceDoesNotBelongToCoach()
        {
            // Arrange
            var (coach1Token, _) = await RegisterAndLoginCoachAsync("owner_coach");
            var (coach2Token, _) = await RegisterAndLoginCoachAsync("other_coach");

            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };
            var createResponse = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: coach1Token);
            var createResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(createResponse);
            var serviceId = createResult.Data!.Id;

            var updateDto = new UpdateCoachInterviewServiceDto
            {
                Price = 2000,
                DurationMinutes = 60
            };

            // Act (coach2 tries to update coach1's service)
            var response = await _api.PutAsync($"/api/v1/coach-interview-services/{serviceId}", updateDto, jwtToken: coach2Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Other coach returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteCoachInterviewService_ReturnsNotFound_WhenServiceDoesNotExist()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginCoachAsync("delete_nonexistent_coach");
            var nonExistentServiceId = Guid.NewGuid();

            // Act
            var response = await _api.DeleteAsync($"/api/v1/coach-interview-services/{nonExistentServiceId}", jwtToken: token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent service ID returns 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task DeleteCoachInterviewService_ReturnsForbidden_WhenServiceDoesNotBelongToCoach()
        {
            // Arrange
            var (coach1Token, _) = await RegisterAndLoginCoachAsync("owner_delete_coach");
            var (coach2Token, _) = await RegisterAndLoginCoachAsync("other_delete_coach");

            var createDto = new CreateCoachInterviewServiceDto
            {
                InterviewTypeId = _cvInterviewTypeId,
                Price = 1500,
                DurationMinutes = 45
            };
            var createResponse = await _api.PostAsync("/api/v1/coach-interview-services", createDto, jwtToken: coach1Token);
            var createResult = await _api.LogDeserializeJson<CoachInterviewServiceDto>(createResponse);
            var serviceId = createResult.Data!.Id;

            // Act (coach2 tries to delete coach1's service)
            var response = await _api.DeleteAsync($"/api/v1/coach-interview-services/{serviceId}", jwtToken: coach2Token, logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "Other coach returns 403 Forbidden");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "CoachInterviewService")]
        public async Task GetCoachInterviewServicesByCoachId_ReturnsNotFound_WhenCoachDoesNotExist()
        {
            // Arrange
            var nonExistentCoachId = Guid.NewGuid();

            // Act
            var response = await _api.GetAsync($"/api/v1/coach-interview-services/coach/{nonExistentCoachId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent coach ID returns 404 Not Found");
        }
    }
}
