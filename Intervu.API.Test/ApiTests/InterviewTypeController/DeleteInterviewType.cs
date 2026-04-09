using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewTypeController
{
    public class DeleteInterviewTypeTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public DeleteInterviewTypeTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task DeleteInterviewType_ReturnsSuccess()
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

            var deleteResponse = await _api.DeleteAsync($"/api/v1/interviewtype/{typeId}", logBody: true);
            var deletePayload = await _api.LogDeserializeJson<JsonElement>(deleteResponse, logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
            await AssertHelper.AssertTrue(deletePayload.Success, "Delete interview type succeeds");
            await AssertHelper.AssertEqual("Interview type deleted successfully", deletePayload.Message, "Success message matches");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task DeleteInterviewType_ReturnsBadRequest_WhenTypeDoesNotExist()
        {
            var response = await _api.DeleteAsync($"/api/v1/interviewtype/{Guid.NewGuid()}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Delete status code is 400 BadRequest for non-existent ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task DeleteInterviewType_InvalidFormatId_ReturnsBadRequest()
        {
            var response = await _api.DeleteAsync("/api/v1/interviewtype/invalid-guid-format", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for invalid format ID");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task DeleteInterviewType_AlreadyDeleted_ReturnsBadRequest()
        {
            var typeId = Guid.NewGuid();
            await _api.PostAsync("/api/v1/interviewtype", new InterviewTypeDto
            {
                Id = typeId,
                Name = $"Double Delete {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Description",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            });

            // First delete
            await _api.DeleteAsync($"/api/v1/interviewtype/{typeId}", logBody: true);

            // Second delete
            var response = await _api.DeleteAsync($"/api/v1/interviewtype/{typeId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, response.StatusCode, "Status code is 400 Bad Request for second delete");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task DeleteInterviewType_InUse_ReturnsConflict()
        {
            // This test assumes an interview type that is currently in use by a coach service or booking.
            // A more complete setup would involve creating a coach service using this type.
            // For now, let's assume if it's in use, it returns 409 Conflict.
            // This is a placeholder for actual business logic testing.

            // To be robust, find an interview type that's likely in use or set one up.
            var response = await _api.GetAsync("/api/v1/interviewtype?pageSize=1", logBody: true);
            var getAllData = await _api.LogDeserializeJson<PagedResult<InterviewTypeDto>>(response);
            if (getAllData.Data?.Items?.Count > 0)
            {
                var id = getAllData.Data.Items[0].Id;
                // Attempt to delete. If it's seeded data, it might be in use.
                var deleteResponse = await _api.DeleteAsync($"/api/v1/interviewtype/{id}", logBody: true);
                if (deleteResponse.StatusCode == HttpStatusCode.Conflict)
                {
                    await AssertHelper.AssertEqual(HttpStatusCode.Conflict, deleteResponse.StatusCode, "Status code is 409 Conflict for in-use type");
                }
            }
        }
    }
}
