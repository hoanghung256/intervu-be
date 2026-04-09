using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewBooking;
using Intervu.Application.DTOs.User;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class ViewMyPaymentHistoryTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        public ViewMyPaymentHistoryTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output) => _api = new ApiHelper(factory.CreateClient());

        [Fact]
        public async Task Handle_AuthenticatedUser_ReturnsPaymentHistory()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var body = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(response);
            await AssertHelper.AssertTrue(body.Success, "Payment history request succeeds");
            await AssertHelper.AssertNotNull(body.Data, "Payment history data is returned");
        }

        [Fact]
        public async Task Handle_MissingToken_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        public async Task Handle_InvalidPageNumber_ReturnsBadRequest()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=0&pageSize=10", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for page 0");
        }

        [Fact]
        public async Task Handle_EmptyHistory_ReturnsSuccess()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for empty history");
            var body = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(response);
            await AssertHelper.AssertEqual(0, body.Data?.Items?.Count, "History items should be empty");
        }

        [Fact]
        public async Task Handle_LargePageSize_EnforcesLimit()
        {
            var login = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var token = (await _api.LogDeserializeJson<LoginResponse>(login)).Data!.Token;
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=1000", jwtToken: token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for large page size");
            var body = await _api.LogDeserializeJson<PagedResult<JsonElement>>(response);
            await AssertHelper.AssertTrue(body.Data?.Items?.Count <= 100, "Should limit items per page");
        }
    }
}
