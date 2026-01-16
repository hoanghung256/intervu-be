using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Skill;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests
{
    public class SkillsControllerTest : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public SkillsControllerTest(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "Smoke")]
        public async Task GetAllSkills_ReturnsSuccessAndData()
        {            
            LogInfo("Getting all skills.");
            var response = await _api.GetAsync("/api/v1/skills?page=1&pageSize=10");
            
            LogInfo("Verify json content is not null and not empty");
            var apiResponse = await _api.LogDeserializeJson<PagedResult<SkillDto>>(response);

            await AssertHelper.AssertNotNull(apiResponse.Data?.Items, "Json content is not null");
            await AssertHelper.AssertNotEmpty(apiResponse.Data?.Items!, "Json content is not empty");
        }
    }
}