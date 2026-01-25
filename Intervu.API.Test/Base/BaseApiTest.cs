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
            //builder.ConfigureServices(services =>
            //{
            //    var sp = services.BuildServiceProvider();
            //    using (var scope = sp.CreateScope())
            //    {
            //        var scopedServices = scope.ServiceProvider;
            //        var db = scopedServices.GetRequiredService<IntervuPostgreDbContext>();
            //        db.Database.EnsureCreated();
            //    }
            //});
        }
    }
}