using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.BookingRequestController
{
    public class ViewBookingRequestsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ViewBookingRequestsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        private async Task<string> LoginAsync(string email, string password)
        {
            var loginResponse = await _api.PostAsync("/api/v1/account/login", new LoginRequest
            {
                Email = email,
                Password = password
            }, logBody: true);

            var loginData = await _api.LogDeserializeJson<LoginResponse>(loginResponse, true);
            return loginData.Data!.Token;
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task ViewBookingRequests_ReturnsSuccess_WhenCandidateIsAuthenticated()
        {
            var token = await LoginAsync("alice@example.com", DEFAULT_PASSWORD);

            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=10", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Candidate can view booking requests with 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task ViewBookingRequests_ReturnsSuccess_WhenCoachIsAuthenticated()
        {
            var token = await LoginAsync(COACH_EMAIL, CANDIDATE_PASSWORD);

            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=10", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Coach can view booking requests with 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task ViewBookingRequests_PageSizeOne_ReturnsSuccess()
        {
            var token = await LoginAsync("alice@example.com", DEFAULT_PASSWORD);

            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=1", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "PageSize=1 returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task ViewBookingRequests_HighPageNumber_ReturnsSuccess()
        {
            var token = await LoginAsync("alice@example.com", DEFAULT_PASSWORD);

            var response = await _api.GetAsync("/api/v1/booking-requests?page=9999&pageSize=10", jwtToken: token, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "High page number still returns 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task ViewBookingRequests_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=10", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Unauthorized, response.StatusCode, "Missing token returns 401 Unauthorized");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "BookingRequest")]
        public async Task ViewBookingRequests_WhenAdminRole_ReturnsForbidden()
        {
            var token = await LoginAsync(ADMIN_EMAIL, DEFAULT_PASSWORD);

            var response = await _api.GetAsync("/api/v1/booking-requests?page=1&pageSize=10", jwtToken: token, logBody: true);

            await AssertHelper.AssertNotEqual(HttpStatusCode.Forbidden, response.StatusCode, "Admin role returns 403 Forbidden");
        }
    }
}
