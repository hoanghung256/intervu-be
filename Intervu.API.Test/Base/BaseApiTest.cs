using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext; // Adjust namespace if necessary
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Intervu.API.Test.Base
{
    public class BaseApiTest<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // 1. Find the existing DbContext registration (SQL Server)
                // var descriptor = services.SingleOrDefault(
                //     d => d.ServiceType == typeof(DbContextOptions<IntervuPostgreDbContext>));

                // 2. Remove it so we don't hit the real database
                // if (descriptor != null)
                // {
                //     services.Remove(descriptor);
                // }

                // 3. Add In-Memory Database for testing purposes
                // services.AddDbContext<IntervuPostgreDbContext>(options =>
                // {
                //     options.UseInMemoryDatabase("InMemoryDbForTesting");
                // });

                // 4. Build the service provider to trigger the seeding defined in OnModelCreating
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<IntervuPostgreDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        }
    }
}