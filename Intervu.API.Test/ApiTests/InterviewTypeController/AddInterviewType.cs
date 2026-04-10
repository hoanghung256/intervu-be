using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewTypeController
{
    // IC-26
    public class AddInterviewTypeTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public AddInterviewTypeTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task AddInterviewType_ReturnsSuccess()
        {
            var createDto = new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = $"New Interview Type {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Description for testing",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            };

            var createResponse = await _api.PostAsync("/api/v1/interviewtype", createDto, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");
            await AssertHelper.AssertTrue(createPayload.Success, "Create interview type succeeds");
            await AssertHelper.AssertEqual("Interview type added successfully", createPayload.Message, "Success message matches");
            await AssertHelper.AssertNotNull(createPayload.Data, "Created interview type data is returned");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task AddInterviewType_DuplicateName_ReturnsConflict()
        {
            var interviewTypeName = $"Duplicate Type {Guid.NewGuid().ToString().Substring(0, 8)}";
            var createDto1 = new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = interviewTypeName,
                Description = "Description for duplicate test",
                SuggestedDurationMinutes = 30,
                MinPrice = 500,
                MaxPrice = 1000
            };

            // First creation should succeed
            await _api.PostAsync("/api/v1/interviewtype", createDto1, logBody: true);

            // Second creation with the same name should fail
            var createDto2 = new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = interviewTypeName,
                Description = "Another description",
                SuggestedDurationMinutes = 60,
                MinPrice = 750,
                MaxPrice = 1500
            };

            var createResponse = await _api.PostAsync("/api/v1/interviewtype", createDto2, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, createResponse.StatusCode, "Status code is 409 Conflict for duplicate name");
            await AssertHelper.AssertFalse(createPayload.Success, "Duplicate interview type creation should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task AddInterviewType_InvalidData_ReturnsBadRequest()
        {
            var createDto = new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = "", // Invalid: Empty name
                Description = "Description for invalid data test",
                SuggestedDurationMinutes = 0, // Invalid: Zero duration
                MinPrice = -100, // Invalid: Negative price
                MaxPrice = 5000
            };

            var createResponse = await _api.PostAsync("/api/v1/interviewtype", createDto, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, createResponse.StatusCode, "Status code is 400 Bad Request for invalid data");
            await AssertHelper.AssertFalse(createPayload.Success, "Invalid data interview type creation should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task AddInterviewType_DurationNotMultipleOf30_ReturnsBadRequest()
        {
            var createDto = new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = $"Invalid Duration {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Duration validation test",
                SuggestedDurationMinutes = 50,
                MinPrice = 1000,
                MaxPrice = 2000
            };

            var createResponse = await _api.PostAsync("/api/v1/interviewtype", createDto, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, createResponse.StatusCode, "Non-multiple duration returns 400 Bad Request");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task AddInterviewType_MinPriceGreaterThanMaxPrice_ReturnsBadRequest()
        {
            var createDto = new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = $"Price Test {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Description for price test",
                SuggestedDurationMinutes = 60,
                MinPrice = 5000,
                MaxPrice = 1000 // Invalid: MinPrice > MaxPrice
            };

            var createResponse = await _api.PostAsync("/api/v1/interviewtype", createDto, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, createResponse.StatusCode, "Status code is 400 Bad Request for invalid price range");
            await AssertHelper.AssertFalse(createPayload.Success, "Invalid price range interview type creation should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task AddInterviewType_LongDescription_ReturnsSuccess()
        {
            var longDescription = new string('A', 500); // Assuming a max length for description
            var createDto = new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = $"Long Desc Type {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = longDescription,
                SuggestedDurationMinutes = 90,
                MinPrice = 2000,
                MaxPrice = 7000
            };

            var createResponse = await _api.PostAsync("/api/v1/interviewtype", createDto, logBody: true);
            var createPayload = await _api.LogDeserializeJson<JsonElement>(createResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK for long description");
            await AssertHelper.AssertTrue(createPayload.Success, "Create interview type succeeds with long description");
        }
    }
}
