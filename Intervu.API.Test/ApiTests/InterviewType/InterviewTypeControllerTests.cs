using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
using Xunit.Abstractions;
using Intervu.Application.DTOs.Common;

namespace Intervu.API.Test.ApiTests.InterviewType
{
    public class InterviewTypeControllerTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public InterviewTypeControllerTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task InterviewType_Lifecycle_ReturnsSuccess()
        {
            // 1. Create Interview Type
            var createDto = new InterviewTypeDto
            {
                Id = Guid.NewGuid(),
                Name = $"New Interview Type {Guid.NewGuid().ToString().Substring(0, 8)}",
                Description = "Description for testing",
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 5000
            };

            LogInfo($"Creating interview type: {createDto.Name}");
            var createResponse = await _api.PostAsync("/api/v1/interviewtype", createDto, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");

            var typeId = createDto.Id;

            // 2. Get All
            LogInfo("Getting all interview types.");
            var getAllResponse = await _api.GetAsync("/api/v1/interviewtype?pageSize=20", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getAllResponse.StatusCode, "Get all status code is 200 OK");
            var getAllData = await _api.LogDeserializeJson<PagedResult<InterviewTypeDto>>(getAllResponse);
            await AssertHelper.AssertTrue(getAllData.Data!.Items.Any(t => t.Id == typeId), "Created type exists in the list");

            // 3. Get By ID
            LogInfo($"Getting interview type by ID {typeId}.");
            var getByIdResponse = await _api.GetAsync($"/api/v1/interviewtype/{typeId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, getByIdResponse.StatusCode, "Get by ID status code is 200 OK");
            var getByIdData = await _api.LogDeserializeJson<InterviewTypeDto>(getByIdResponse);
            await AssertHelper.AssertEqual(createDto.Name, getByIdData.Data!.Name, "Name matches");

            // 4. Update
            var updateDto = new InterviewTypeDto
            {
                Id = typeId,
                Name = createDto.Name + " Updated",
                Description = "Updated description",
                SuggestedDurationMinutes = 45,
                MinPrice = 1200,
                MaxPrice = 4800
            };
            LogInfo($"Updating interview type {typeId}.");
            var updateResponse = await _api.PutAsync($"/api/v1/interviewtype/{typeId}", updateDto, logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");

            // 5. Delete
            LogInfo($"Deleting interview type {typeId}.");
            var deleteResponse = await _api.DeleteAsync($"/api/v1/interviewtype/{typeId}", logBody: true);
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "InterviewType")]
        public async Task GetById_ReturnsNotFound_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            LogInfo($"Getting non-existent interview type {invalidId}.");
            var response = await _api.GetAsync($"/api/v1/interviewtype/{invalidId}", logBody: true);

            // Assert
            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
        }
    }
}