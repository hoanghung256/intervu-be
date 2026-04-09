using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewTypeController
{
    // IC-28
    public class UpdateInterviewTypeTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public UpdateInterviewTypeTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task UpdateInterviewType_ReturnsSuccess()
        {
            var typeId = Guid.NewGuid();
            await _api.PostAsync("/api/v1/interviewtype", new InterviewTypeDto
            {
                Id = typeId,
                Name = $"Type {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Description",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            });

            var updateResponse = await _api.PutAsync($"/api/v1/interviewtype/{typeId}", new InterviewTypeDto
            {
                Id = typeId,
                Name = "Updated Type",
                Description = "Updated description",
                SuggestedDurationMinutes = 45,
                MinPrice = 1200,
                MaxPrice = 4800
            }, logBody: true);

            var updatePayload = await _api.LogDeserializeJson<JsonElement>(updateResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");
            await AssertHelper.AssertTrue(updatePayload.Success, "Update interview type succeeds");
            await AssertHelper.AssertEqual("Interview type updated successfully", updatePayload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task UpdateInterviewType_ReturnsBadRequest_WhenTypeDoesNotExist()
        {
            var missingTypeId = Guid.NewGuid();

            var updateResponse = await _api.PutAsync($"/api/v1/interviewtype/{missingTypeId}", new InterviewTypeDto
            {
                Id = missingTypeId,
                Name = "Updated Type",
                Description = "Updated description",
                SuggestedDurationMinutes = 45,
                MinPrice = 1200,
                MaxPrice = 4800
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, updateResponse.StatusCode, "Update status code is 400 BadRequest for non-existent ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task UpdateInterviewType_InvalidData_ReturnsBadRequest()
        {
            var typeId = Guid.NewGuid();
            await _api.PostAsync("/api/v1/interviewtype", new InterviewTypeDto
            {
                Id = typeId,
                Name = $"Valid Type {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Description",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            });

            var updateResponse = await _api.PutAsync($"/api/v1/interviewtype/{typeId}", new InterviewTypeDto
            {
                Id = typeId,
                Name = "", // Invalid: Empty name
                Description = "Updated description",
                SuggestedDurationMinutes = -10, // Invalid: Negative duration
                MinPrice = 2000,
                MaxPrice = 1000 // Invalid: Min > Max
            }, logBody: true);

            var updatePayload = await _api.LogDeserializeJson<JsonElement>(updateResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, updateResponse.StatusCode, "Status code is 400 Bad Request for invalid update data");
            await AssertHelper.AssertFalse(updatePayload.Success, "Invalid data update should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task UpdateInterviewType_DuplicateName_ReturnsConflict()
        {
            var type1Name = $"Type 1 {Guid.NewGuid().ToString().Substring(0, 8)}";
            var type2Name = $"Type 2 {Guid.NewGuid().ToString().Substring(0, 8)}";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            await _api.PostAsync("/api/v1/interviewtype", new InterviewTypeDto
            {
                Id = id1,
                Name = type1Name,
                Description = "Description 1",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            });
            await _api.PostAsync("/api/v1/interviewtype", new InterviewTypeDto
            {
                Id = id2,
                Name = type2Name,
                Description = "Description 2",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            });

            // Try to update Type 2 to have Type 1's name
            var updateResponse = await _api.PutAsync($"/api/v1/interviewtype/{id2}", new InterviewTypeDto
            {
                Id = id2,
                Name = type1Name, // Duplicate name
                Description = "Description updated",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            }, logBody: true);

            var updatePayload = await _api.LogDeserializeJson<JsonElement>(updateResponse, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.Conflict, updateResponse.StatusCode, "Status code is 409 Conflict for duplicate name update");
            await AssertHelper.AssertFalse(updatePayload.Success, "Update with duplicate name should fail");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task UpdateInterviewType_InvalidIdFormat_ReturnsBadRequest()
        {
            var response = await _api.PutAsync("/api/v1/interviewtype/invalid-guid-format", new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = "Something",
                Description = "Something",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            }, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid format ID in URL");
        }
    }
}
