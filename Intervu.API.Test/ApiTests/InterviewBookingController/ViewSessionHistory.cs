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
    public class ViewSessionHistoryTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewSessionHistoryTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetHistory_ReturnsSuccess()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetHistory_ReturnsUnauthorized_WhenNoToken()
        {
            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Status code is 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetHistory_EmptyResults_ReturnsSuccess()
        {
            // Assuming Bob has no history
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "bob@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=10", jwtToken: loginData.Data!.Token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for empty history");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(response);
            await AssertHelper.AssertTrue(apiResponse.Success, "API response indicates success");
            await AssertHelper.AssertEqual(0, apiResponse.Data?.Items?.Count, "History should be empty for new/clean user");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetHistory_InvalidPage_ReturnsBadRequest()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=-1&pageSize=10", jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for negative page index");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetHistory_LargePageSize_EnforcesLimit()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1&pageSize=5000", jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for large pageSize");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<JsonElement>>(response);
            await AssertHelper.AssertTrue(apiResponse.Data?.Items?.Count <= 100, "Should limit items to reasonable page size");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetHistory_PageBeyondTotal_ReturnsEmptyList()
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest { Email = "alice@example.com", Password = DEFAULT_PASSWORD });
            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse);

            var response = await _api.GetAsync("/api/v1/interview-booking/history?page=1000&pageSize=10", jwtToken: loginData.Data!.Token, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for page beyond total");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<InterviewBookingTransactionHistoryDto>>(response);
            await AssertHelper.AssertEqual(0, apiResponse.Data?.Items?.Count, "Should return empty items when page beyond limit");
        }
    }
}
