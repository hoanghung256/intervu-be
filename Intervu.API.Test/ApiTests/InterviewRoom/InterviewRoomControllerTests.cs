using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Intervu.API.Controllers.v1;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoom
{
    public class InterviewRoomControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public InterviewRoomControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }
        
        // Seeded Data
        private readonly Guid _aliceId = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
        private readonly string _aliceEmail = "alice@example.com";
        private readonly Guid _bobId = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
        // private readonly string _bobEmail = "bob@example.com";

        private async Task<(string token, Guid userId)> LoginSeededUserAsync(string email)
        {
            var password = ACCOUNT_PASSWORD;

            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = password });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            
            return (loginData.Data!.Token, loginData.Data.User.Id);
        }

        //[Fact]
        //[Trait("Category", "API")]
        //[Trait("Category", "InterviewRoom")]
        //public async Task CreateRoom_ReturnsSuccess_WhenOnlyCandidateProvided()
        //{
        //    // Arrange
        //    var (_, candidateId) = await LoginSeededUserAsync(_aliceEmail);
        //    var request = new InterviewRoomController.CreateRoomDto { candidateId = candidateId, coachId = Guid.Empty };

        //    // Act
        //    LogInfo("Creating interview room for candidate only (AI Interview).");
        //    var response = await _api.PostAsync("/api/v1/interviewroom", request, logBody: true);

        //    // Assert
        //    await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        //    var apiResponse = await _api.LogDeserializeJson<RoomData>(response);
        //    await AssertHelper.AssertTrue(apiResponse.Success, "Room creation successful");
        //    await AssertHelper.AssertNotNull(apiResponse.Data?.RoomId, "Room ID returned");
        //}

        //[Fact]
        //[Trait("Category", "API")]
        //[Trait("Category", "InterviewRoom")]
        //public async Task CreateRoom_ReturnsSuccess_WhenCandidateAndCoachProvided()
        //{
        //    // Arrange
        //    var (_, candidateId) = await LoginSeededUserAsync(_aliceEmail);
        //    var coachId = _bobId;
            
        //    var request = new InterviewRoomController.CreateRoomDto 
        //    { 
        //        candidateId = candidateId,
        //        coachId = coachId
        //    };

        //    // Act
        //    LogInfo("Creating interview room for candidate and coach.");
        //    var response = await _api.PostAsync("/api/v1/interviewroom", request, logBody: true);

        //    // Assert
        //    await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        //    var apiResponse = await _api.LogDeserializeJson<RoomData>(response);
        //    await AssertHelper.AssertTrue(apiResponse.Success, "Room creation successful");
        //    await AssertHelper.AssertNotNull(apiResponse.Data?.RoomId, "Room ID returned");
        //}

        //[Fact]
        //[Trait("Category", "API")]
        //[Trait("Category", "InterviewRoom")]
        //public async Task GetList_ReturnsSuccess_WhenUserIsAuthenticated()
        //{
        //    // Arrange
        //    var (token, candidateId) = await LoginSeededUserAsync(_aliceEmail);
            
        //    // Create a room first so the list isn't empty
        //    await _api.PostAsync("/api/v1/interviewroom", new InterviewRoomController.CreateRoomDto { candidateId = candidateId });

        //    // Act
        //    LogInfo("Getting interview room history.");
        //    var response = await _api.GetAsync("/api/v1/interviewroom", jwtToken: token, logBody: true);

        //    // Assert
        //    await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        //    var apiResponse = await _api.LogDeserializeJson<IEnumerable<Domain.Entities.InterviewRoom>>(response);
        //    await AssertHelper.AssertTrue(apiResponse.Success, "Request successful");
        //}

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task GetList_ReturnsUnauthorized_WhenTokenIsMissing()
        {
            // Act
            LogInfo("Getting interview room history without token.");
            var response = await _api.GetAsync("/api/v1/interviewroom", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        private class RoomData { public Guid RoomId { get; set; } }
    }
}