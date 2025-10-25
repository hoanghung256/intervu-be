using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Intervu.API.Properties;
using Intervu.Application;
using Intervu.Infrastructure;
using Microsoft.OpenApi.Models;

namespace Intervu.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- API VERSIONING CONFIGURATION ---
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;

                // Support read version /api/v{version}/controller
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // Format version group: v1, v2, v3
                options.SubstituteApiVersionInUrl = true; // Inject {version} dynamically in [Route] of controller
            });

            // --- CONTROLLERS ---
            builder.Services.AddControllers(options =>
            {
                options.Conventions.Add(new LowercaseControllerRouteConvention());
            });

            // --- SWAGGER CONFIGURATION ---
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Intervu API",
                    Version = "v1"
                });

                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "Intervu API",
                    Version = "v2"
                });
            });

            // --- CUSTOM SERVICES ---
            builder.Services.AddUseCases(builder.Configuration);
            builder.Services.AddPersistenceSqlServer(builder.Configuration);
            builder.Services.AddInfrastructureExternalServices(builder.Configuration);

            // --- CORS ---
            builder.Services.AddCors(options =>
            {
                // Allow all origin when in development
                options.AddPolicy(name: CorsPolicies.DevCorsPolicy, policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });

                // Strict origin when in production
                options.AddPolicy(name: CorsPolicies.ProdCorsPolicy, policy =>
                {
                    var allowedOrigins = builder.Configuration
                        .GetValue<string>("CorsSettings:AllowedOrigins")?
                        .Split(",") ?? [];

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // --- HTTP REQUEST PIPELINE ---
            if (app.Environment.IsDevelopment())
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{description.GroupName}/swagger.json",
                            $"Intervu API {description.GroupName.ToUpperInvariant()}"
                        );
                    }
                });

                app.UseCors(CorsPolicies.DevCorsPolicy);
            } else
            {
                app.UseCors(CorsPolicies.ProdCorsPolicy);
            }

                app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
