using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
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
            await AssertHelper.AssertEqual(HttpStatusCode.OK, deleteResponse.StatusCode, "Delete status code is 200 OK");
        }
    }
}
