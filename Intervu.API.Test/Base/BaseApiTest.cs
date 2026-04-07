using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext; // Adjust namespace if necessary
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// Forces all tests in this assembly to run sequentially.
// This prevents race conditions on seeded data/DB and reduces memory usage to prevent OOM.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace Intervu.API.Test.Base
{
    public class BaseApiTest<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        public BaseApiTest()
        {
            var firebasePath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "Intervu.API", "firebase.json"));

            Environment.SetEnvironmentVariable("Firebase__CredentialPath", firebasePath);
            Environment.SetEnvironmentVariable("Firebase__StorageBucket", "intervu-test-bucket");
            Environment.SetEnvironmentVariable("JwtConfig__Key", "5ee9e2d9d2bdc8c4848ea37c95044fd5");
            Environment.SetEnvironmentVariable("PayOS__Payment__ClientId", "test-client");
            Environment.SetEnvironmentVariable("PayOS__Payment__ApiKey", "test-api-key");
            Environment.SetEnvironmentVariable("PayOS__Payment__ChecksumKey", "test-checksum-key");
            Environment.SetEnvironmentVariable("PayOS__Payment__ReturnEndpoint", "http://localhost/return");
            Environment.SetEnvironmentVariable("PayOS__Payment__CancelEndpoint", "http://localhost/cancel");
            Environment.SetEnvironmentVariable("PayOS__Payout__ClientId", "test-client");
            Environment.SetEnvironmentVariable("PayOS__Payout__ApiKey", "test-api-key");
            Environment.SetEnvironmentVariable("PayOS__Payout__ChecksumKey", "test-checksum-key");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                var firebasePath = Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..", "..", "Intervu.API", "firebase.json"));

                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Firebase:CredentialPath"] = firebasePath,
                    ["Firebase:StorageBucket"] = "intervu-test-bucket"
                });
            });
        }

        // Ensure the client doesn't hang forever if the API deadlocks
        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            client.Timeout = TimeSpan.FromSeconds(100); // Fail test if API takes > 100s
        }
    }
}