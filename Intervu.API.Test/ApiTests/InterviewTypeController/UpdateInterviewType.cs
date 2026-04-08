using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewTypeController
{
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

            await AssertHelper.AssertEqual(HttpStatusCode.OK, updateResponse.StatusCode, "Update status code is 200 OK");
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

            await AssertHelper.AssertEqual(HttpStatusCode.BadRequest, updateResponse.StatusCode, "Update status code is 400 BadRequest");
        }
    }
}
