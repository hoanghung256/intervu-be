using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.RescheduleRequest;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    public class ApproveRescheduleRequestTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _rescheduleRequestId = Guid.Parse("f1f1f1f1-6666-4a1a-8a1a-666666666666");
        public ApproveRescheduleRequestTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        public async Task Handle_AuthorizedCoach_ApprovesRequest()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Response status code is 200 OK");
        }

        [Fact]
        public async Task Handle_MissingToken_ReturnsUnauthorized()
        {
            var response = await _api.PostAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}/respond", new RespondToRescheduleRequestDto { IsApproved = true }, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Response status code is 401 Unauthorized");
        }
    }
}
