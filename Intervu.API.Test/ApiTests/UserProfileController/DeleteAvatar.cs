using Intervu.API.Test.Base;
using Intervu.API.Test.Utils;
using System.Net;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.UserProfileController
{
    public class DeleteAvatarTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        private readonly ApiHelper _api;

        public DeleteAvatarTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
            _api = new ApiHelper(factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "API")]
        [Trait("Category", "UserProfile")]
        public async Task DeleteAvatar_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var nonExistentUserId = Guid.NewGuid();

            LogInfo($"Attempting to delete avatar for non-existent user {nonExistentUserId}.");
            var response = await _api.DeleteAsync($"/api/v1/userprofile/delete-avatar/{nonExistentUserId}", logBody: true);

            await AssertHelper.AssertEqual(HttpStatusCode.NotFound, response.StatusCode, "Status code is 404 Not Found");
        }
    }
}
