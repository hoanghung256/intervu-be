using Intervu.API.Properties;
using Intervu.Application;
using Intervu.Infrastructure;

namespace Intervu.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            builder.Services.AddUseCases(builder.Configuration);

            builder.Services.AddPersistenceSqlServer(builder.Configuration);

            builder.Services.AddInfrastructureExternalServices(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                // Development CORS policy - allow all
                options.AddPolicy(name: CorsPolicies.DevCorsPolicy,
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                );

                // Production CORS policy - restrict to specific origins
                options.AddPolicy(name: CorsPolicies.ProdCorsPolicy,
                    policy =>
                    {
                        var allowedOrigins = builder.Configuration
                                            .GetValue<string>("CorsSettings:AllowedOrigins")?
                                            .Split(",") ?? [];

                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                );
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
