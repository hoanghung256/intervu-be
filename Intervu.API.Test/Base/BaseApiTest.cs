using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext; // Adjust namespace if necessary
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// Forces all tests in this assembly to run sequentially.
// This prevents race conditions on seeded data/DB and reduces memory usage to prevent OOM.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace Intervu.API.Test.Base
{
    public class BaseApiTest<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }

        // Ensure the client doesn't hang forever if the API deadlocks
        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            client.Timeout = TimeSpan.FromSeconds(100); // Fail test if API takes > 100s
        }
    }
}