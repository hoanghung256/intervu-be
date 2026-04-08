using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests.AvailabilitiesController
{
    public class ViewAvailabilitySlotsTests : BaseTest, IClassFixture<BaseApiTest<Program>>
    {
        public ViewAvailabilitySlotsTests(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
        {
        }

        [Fact(Skip = "No dedicated availability-list endpoint is covered by existing availability API tests; add when contract is confirmed.")]
        [Trait("Category", "API")]
        [Trait("Category", "Availability")]
        public Task ViewAvailabilitySlots_Placeholder()
        {
            return Task.CompletedTask;
        }
    }
}
