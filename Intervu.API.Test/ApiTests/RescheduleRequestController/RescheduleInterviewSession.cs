using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    // IC-34
    public class RescheduleInterviewSessionTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _roomRescheduleCreateId = Guid.Parse("b1b1b1b1-2222-4a1a-8a1a-222222222222");

        public RescheduleInterviewSessionTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAsAliceAsync()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_WithValidData_ReturnsSuccess()
        {
            var token = await LoginAsAliceAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                NewStartTime = DateTime.UtcNow.AddDays(2),
                Reason = "Need to reschedule due to personal emergency that requires my immediate attention"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task CreateRescheduleRequest_WithShortReason_ReturnsBadRequest()
        {
            var token = await LoginAsAliceAsync();
            var response = await _api.PostAsync("/api/v1/reschedule-requests", new CreateRescheduleRequestDto
            {
                RoomId = _roomRescheduleCreateId,
                NewStartTime = DateTime.UtcNow.AddDays(2),
                Reason = "Short"
            }, jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request");
        }
    }
}
