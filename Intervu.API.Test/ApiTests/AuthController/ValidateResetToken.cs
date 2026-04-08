using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    public class ValidateResetTokenTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public ValidateResetTokenTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task ValidateResetToken_ReturnsBadRequest_WhenTokenIsInvalid()
        {
            LogInfo("Validating a fake reset token.");
            var response = await _api.GetAsync("/api/v1/auth/validate-reset-token/fake-token-123", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");

            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Validation failed");
            await AssertHelper.AssertNotNull(apiResponse.Message, "Error message is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ValidateResetToken_WhitespaceToken_ReturnsBadRequest()
        {
            var response = await _api.GetAsync("/api/v1/auth/validate-reset-token/%20", logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertFalse(apiResponse.Success, "Validation should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ValidateResetToken_MissingTokenRoute_ReturnsNotFound()
        {
            var response = await _api.GetAsync("/api/v1/auth/validate-reset-token/", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 NotFound when route token is missing");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ValidateResetToken_VeryLongToken_ReturnsBadRequest()
        {
            // Arrange – generate a token far exceeding any real token length (boundary: ~500 chars)
            var longToken = new string('x', 500);

            // Act
            LogInfo("Validating an over-length reset token (boundary test).");
            var response = await _api.GetAsync($"/api/v1/auth/validate-reset-token/{longToken}", logBody: true);

            // Assert – the token is invalid regardless of length
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Over-length token returns 400 BadRequest");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Validation fails for over-length token");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ValidateResetToken_NumericOnlyToken_ReturnsBadRequest()
        {
            // Arrange – numeric-only string is not a valid reset token
            const string numericToken = "1234567890";

            // Act
            LogInfo("Validating a numeric-only reset token.");
            var response = await _api.GetAsync($"/api/v1/auth/validate-reset-token/{numericToken}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Numeric-only token returns 400 BadRequest");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Validation fails for numeric-only token");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_ValidateResetToken_SpecialCharToken_ReturnsBadRequest()
        {
            // Arrange – URL-encoded special chars similar to base64 padding (%, +, =)
            // Using URL-safe encoding: %2B = '+', %3D = '='
            const string specialToken = "abc%2Bdef%3Dghi%2Fjkl";

            // Act
            LogInfo("Validating a token with URL-encoded special characters.");
            var response = await _api.GetAsync($"/api/v1/auth/validate-reset-token/{specialToken}", logBody: true);

            // Assert – the decoded token is still not a valid database token
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Special-char token returns 400 BadRequest");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Validation fails for special-char token");
        }
    }
}
