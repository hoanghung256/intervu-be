using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.Exceptions;
using Intervu.Application.DTOs.User;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    public class SignInWithGoogleTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SignInWithGoogleTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_ReturnsBadRequest_WhenTokenMissing()
        {
            var exception = await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _api.PostAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = "" }, logBody: true));

            await AssertHelper.AssertEqual("IdToken (or credential) is required", exception.Message, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_ReturnsBadRequest_WhenBodyIsNull()
        {
            var exception = await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _api.PostAsync<GoogleLoginRequest?>("/api/v1/auth/google", null, logBody: true));

            await AssertHelper.AssertEqual("IdToken (or credential) is required", exception.Message, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_ReturnsBadRequest_WhenIdTokenIsWhitespace()
        {
            // Arrange – whitespace-only token is effectively empty after trim
            var request = new GoogleLoginRequest { IdToken = "   " };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _api.PostAsync("/api/v1/auth/google", request, logBody: true));

            await AssertHelper.AssertEqual("IdToken (or credential) is required", exception.Message, "Whitespace token treated as missing");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_ReturnsBadRequest_WhenIdTokenIsPlausibleButInvalid()
        {
            // Arrange – a JWT-shaped string (three dot-separated segments) that is not a valid Google token
            const string fakeJwtStyleToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTYiLCJlbWFpbCI6ImZha2VAZXhhbXBsZS5jb20ifQ.invalidsignature";
            var request = new GoogleLoginRequest { IdToken = fakeJwtStyleToken };

            // Act & Assert – the API rejects the token because Google verification fails
            var exception = await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _api.PostAsync("/api/v1/auth/google", request, logBody: true));

            await AssertHelper.AssertNotNull(exception.Message, "BadRequestException raised for invalid Google token");
        }
    }
}
