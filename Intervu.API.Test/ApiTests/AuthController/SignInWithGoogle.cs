using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AuthController
{
    // IC-1
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
        public async Task GoogleLogin_Abnormal_TokenMissing_ReturnsBadRequest()
        {
            var response = await _api.PostAsync("/api/v1/auth/google", new GoogleLoginRequest { IdToken = "" }, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Missing token returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_Abnormal_BodyNull_ReturnsBadRequest()
        {
            var response = await _api.PostAsync<GoogleLoginRequest?>("/api/v1/auth/google", null, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Null body returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_Boundary_IdTokenWhitespace_ReturnsBadRequest()
        {
            // Arrange – whitespace-only token is effectively empty after trim
            var request = new GoogleLoginRequest { IdToken = "   " };

            var response = await _api.PostAsync("/api/v1/auth/google", request, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Whitespace token returns 400 BadRequest");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task GoogleLogin_Abnormal_IdTokenInvalidSignature_ReturnsBadRequest()
        {
            // Arrange – a JWT-shaped string (three dot-separated segments) that is not a valid Google token
            const string fakeJwtStyleToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTYiLCJlbWFpbCI6ImZha2VAZXhhbXBsZS5jb20ifQ.invalidsignature";
            var request = new GoogleLoginRequest { IdToken = fakeJwtStyleToken };

            var response = await _api.PostAsync("/api/v1/auth/google", request, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Invalid Google token returns 400 BadRequest");
        }
    }
}
