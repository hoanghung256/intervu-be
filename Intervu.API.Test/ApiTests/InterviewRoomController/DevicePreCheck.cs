using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewRoomController
{
    public class DevicePreCheckTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _existingRoomId = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"); // Assuming this is a valid room ID for testing

        public DevicePreCheckTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginUserAsync(string email)
        {
            var response = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = email, Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(response);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DevicePreCheck_Success_ReturnsOk()
        {
            var token = await LoginUserAsync("bob@example.com"); // Assuming Bob is part of _existingRoomId

            var response = await _api.GetAsync($"/api/v1/interview-room/{_existingRoomId}/device-precheck", jwtToken: token, logBody: true);
            var payload = await _api.LogDeserializeJson<JsonElement>(response, true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(payload.Success, "Device pre-check successful");
            // Further assertions can be added here to check the structure of the payload.Data
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DevicePreCheck_Unauthorized_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync($"/api/v1/interview-room/{_existingRoomId}/device-precheck", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Unauthenticated user should get 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DevicePreCheck_NonExistentRoom_ReturnsNotFound()
        {
            var token = await LoginUserAsync("bob@example.com");
            var nonExistentRoomId = Guid.NewGuid();

            var response = await _api.GetAsync($"/api/v1/interview-room/{nonExistentRoomId}/device-precheck", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Non-existent room ID should return 404 Not Found");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DevicePreCheck_InvalidRoomIdFormat_ReturnsBadRequest()
        {
            var token = await LoginUserAsync("bob@example.com");
            var invalidRoomId = "not-a-valid-guid";

            var response = await _api.GetAsync($"/api/v1/interview-room/{invalidRoomId}/device-precheck", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid room ID format should return 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewRoom")]
        public async Task DevicePreCheck_UserNotInRoom_ReturnsForbidden()
        {
            var token = await LoginUserAsync("alice@example.com"); // Assuming Alice is NOT part of _existingRoomId

            var response = await _api.GetAsync($"/api/v1/interview-room/{_existingRoomId}/device-precheck", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Forbidden, response.StatusCode, "User not in room should get 403 Forbidden");
        }
    }
}
