using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.RescheduleRequestController
{
    public class ViewRescheduleRequestTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly Guid _rescheduleRequestId = Guid.Parse("f1f1f1f1-6666-4a1a-8a1a-666666666666");

        public ViewRescheduleRequestTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task GetRescheduleRequestById_ReturnsSuccess_WhenIdIsValid()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync($"/api/v1/reschedule-requests/{_rescheduleRequestId}", jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task GetMyRescheduleRequests_ReturnsSuccess_WhenAuthenticated()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync("/api/v1/reschedule-requests/my-requests", jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "RescheduleRequest")]
        public async Task GetPendingResponses_ReturnsSuccess_WhenAuthenticated()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync("/api/v1/reschedule-requests/pending-responses", jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
        }
    }
}
