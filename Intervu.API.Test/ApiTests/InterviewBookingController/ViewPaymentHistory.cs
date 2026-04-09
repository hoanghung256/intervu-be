using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewBookingController
{
    public class ViewPaymentHistoryTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly HttpClient _client;

        public ViewPaymentHistoryTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _client = factory.CreateClient();
            _api = new ApiHelper(_client);
        }

        // ── GET /interview-booking/{orderCode} ─────────────────────────────────
        // No authentication required for this endpoint.

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetTransaction_ReturnsSuccess_WhenOrderCodeDoesNotExist()
        {
            // Arrange – a very large orderCode is guaranteed not to match any seeded row
            const int nonExistentOrderCode = 999999;

            // Act – no auth header needed
            var response = await _api.GetAsync($"/api/v1/interview-booking/{nonExistentOrderCode}", logBody: true);

            // Assert – controller always wraps the (null) result in 200 OK
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK even for unknown orderCode");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Response success flag is true");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task GetTransaction_ReturnsSuccess_WhenOrderCodeIsZero()
        {
            // Arrange – boundary: orderCode = 0 (no matching row expected)
            // Act
            var response = await _api.GetAsync("/api/v1/interview-booking/0", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK for orderCode=0");
            var payload = await _api.LogDeserializeJson<JsonElement>(response, logBody: true);
            await AssertHelper.AssertTrue(payload.Success, "Response success flag is true for zero orderCode");
        }

        // ── POST /interview-booking/webhook ────────────────────────────────────
        // No authentication required. Any exceptions in the handler are caught and
        // returned as Ok(ex.Message), so the endpoint always responds 200.

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task VerifyPaymentAsync_Returns200_WhenPayloadIsProvided()
        {
            // Arrange – a minimal JSON body that satisfies model binding for the Webhook type;
            // the handler will throw internally (invalid signature), which is caught and returned as 200.
            var minimalWebhookBody = new StringContent("{}", Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/interview-booking/webhook")
            {
                Content = minimalWebhookBody
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert – exceptions are caught inside the controller; endpoint always returns 200
            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Webhook endpoint returns 200 regardless of payload validity");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewBooking")]
        public async Task VerifyPaymentAsync_ReturnsBadRequest_WhenBodyIsMissing()
        {
            // Arrange – send a POST with no body and wrong content-type; model binding should fail
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/interview-booking/webhook");
            // No Content set — ASP.NET Core model binding will return 400 for a required complex type

            // Act
            var response = await _client.SendAsync(request);

            // Assert – without a body the framework cannot bind the Webhook parameter → 400
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing body returns 400 Bad Request");
        }
    }
}
