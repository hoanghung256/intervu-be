using Intervu.API.Test.Base;
using Xunit.Abstractions;

namespace Intervu.API.Test.ApiTests
{
    public class ApiSmokeTests : BaseTest, IClassFixture<BaseApiTest<Intervu.API.Program>>
    {
        private readonly HttpClient _client;

        public ApiSmokeTests(BaseApiTest<Intervu.API.Program> factory, ITestOutputHelper output) : base(output)
        {
            _client = factory.CreateClient();
        }

        //[Fact]
        //public async Task GetWeatherForecast_ReturnsSuccessStatusCode()
        //{
        //    await LogStepAsync(async () =>
        //    {
        //        // Act
        //        var response = await _client.GetAsync("/WeatherForecast");

        //        // Assert
        //        response.EnsureSuccessStatusCode(); // Status Code 200-299
        //        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        //    });
        //}
        
        //[Fact]
        //public async Task Swagger_ReturnsSuccessStatusCode()
        //{
        //     await LogStepAsync(async () =>
        //    {
        //        // Act
        //        // Swagger UI is usually at /swagger/index.html or similar
        //        // The JSON is at /swagger/v1/swagger.json
        //        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        //        // Assert
        //        response.EnsureSuccessStatusCode();
        //        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        //    });
        //}
    }
}
