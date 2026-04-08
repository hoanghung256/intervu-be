using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.User;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AccountController
{
    public class SignUpTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;
        private readonly string _aliceEmail = "alice@example.com";

        public SignUpTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        [Trait("Category", "Smoke")]
        public async Task Register_ReturnsSuccess_WhenDataIsValid()
        {
            var registerRequest = new RegisterRequest
            {
                Email = $"test_{Guid.NewGuid()}@example.com",
                Password = CANDIDATE_PASSWORD,
                FullName = "Test_User",
            };

            LogInfo("Registering new user.");
            var response = await _api.PostAsync("/api/v1/account/register", registerRequest, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, response.StatusCode, "Status code is 200 OK");
            await AssertHelper.AssertTrue(apiResponse.Success, "Registration successful");
            await AssertHelper.AssertEqual("Registration successful", apiResponse.Message, "Message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_Register_MissingEmail_ReturnsBadRequestWithValidationMessage()
        {
            var request = new RegisterRequest
            {
                Email = "",
                Password = CANDIDATE_PASSWORD,
                FullName = "Missing Email User"
            };

            var response = await _api.PostAsync("/api/v1/account/register", request, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertFalse(apiResponse.Success, "Registration should fail");
            await AssertHelper.AssertEqual("Email is required", apiResponse.Message, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Handle_Register_MissingPassword_ReturnsBadRequestWithValidationMessage()
        {
            var request = new RegisterRequest
            {
                Email = $"nopass_{Guid.NewGuid()}@example.com",
                Password = "",
                FullName = "Missing Password User"
            };

            var response = await _api.PostAsync("/api/v1/account/register", request, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertFalse(apiResponse.Success, "Registration should fail");
            await AssertHelper.AssertEqual("Password is required", apiResponse.Message, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            var request = new RegisterRequest
            {
                Email = _aliceEmail,
                Password = CANDIDATE_PASSWORD,
                FullName = "Duplicate User"
            };

            LogInfo("Registering same user again.");
            var response = await _api.PostAsync("/api/v1/account/register", request, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            var apiResponse = await _api.LogDeserializeJson<object>(response);
            await AssertHelper.AssertFalse(apiResponse.Success, "Registration failed");
            await AssertHelper.AssertEqual("Email already exists", apiResponse.Message, "Conflict message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Register_ReturnsBadRequest_WhenPasswordIsWeak()
        {
            var request = new RegisterRequest
            {
                Email = $"weak_{Guid.NewGuid()}@example.com",
                Password = "123",
                FullName = "Weak Password User"
            };

            LogInfo("Registering user with weak password.");
            var response = await _api.PostAsync("/api/v1/account/register", request, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertFalse(apiResponse.Success, "Registration should fail");
            await AssertHelper.AssertEqual("Password must be at least 8 characters", apiResponse.Message, "Validation message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Authentication")]
        public async Task Register_ReturnsBadRequest_WhenEmailIsInvalid()
        {
            var request = new RegisterRequest
            {
                Email = "invalid-email-format",
                Password = CANDIDATE_PASSWORD,
                FullName = "Invalid Email User"
            };

            LogInfo("Registering user with invalid email.");
            var response = await _api.PostAsync("/api/v1/account/register", request, logBody: true);
            var apiResponse = await _api.LogDeserializeJson<object>(response);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 BadRequest");
            await AssertHelper.AssertFalse(apiResponse.Success, "Registration should fail");
            await AssertHelper.AssertEqual("Invalid email format", apiResponse.Message, "Validation message matches");
        }
    }
}
