using Intervu.API.Test.Reporting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

// Forces all tests in this assembly to run sequentially.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace Intervu.API.Test.Base
{
    public class BaseApiTest<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            // Add diagnostic logging to catch "System Exceptions" during API startup
            builder.ConfigureKestrel(options => { }); // Example of touching builder
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            try
            {
                return base.CreateHost(builder);
            }
            catch (Exception ex)
            {
                // Log the failure to the Extent "Infrastructure" node
                var systemNode = ExtentService.Instance.CreateTest("Infrastructure: API Startup Failure");
                systemNode.Fail($"The API failed to start in the Testing environment.<br/><b>Error:</b> {ex.Message}<br/><pre>{ex.StackTrace}</pre>");
                ExtentService.Instance.Flush();
                throw;
            }
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            client.Timeout = TimeSpan.FromSeconds(100);
        }
    }
}
