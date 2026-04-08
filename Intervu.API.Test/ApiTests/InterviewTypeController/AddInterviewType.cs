using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.InterviewType;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.InterviewTypeController
{
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
            await AssertHelper.AssertEqual(HttpStatusCode.OK, createResponse.StatusCode, "Create status code is 200 OK");
        }
    }
}
