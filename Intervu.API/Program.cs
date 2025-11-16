using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Intervu.API.Hubs;
using Intervu.Application;
using Intervu.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Intervu.Domain.Entities.Constants;
using Intervu.API.Utils.Constant;
using Intervu.API.Utils;

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
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
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

            // --- AUTHENTICATION WITH JWT CONFIGURATION ---
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
                    ValidAudience = builder.Configuration["JwtConfig:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

            // --- AUTHORIZATION POLICIES ---
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    AuthorizationPolicies.Admin,
                    policy => policy.RequireRole(UserRole.Admin.ToString())
                );

                options.AddPolicy(
                    AuthorizationPolicies.Interviewer,
                    policy => policy.RequireRole(UserRole.Interviewer.ToString())
                );

                options.AddPolicy(
                    AuthorizationPolicies.Interviewee,
                    policy => policy.RequireRole(UserRole.Interviewee.ToString())
                );

                options.AddPolicy(
                    AuthorizationPolicies.IntervieweeOrInterviewer,
                    policy => policy.RequireAssertion(context =>
                        context.User.IsInRole(UserRole.Interviewee.ToString()) ||
                        context.User.IsInRole(UserRole.Interviewer.ToString()))
                );

                options.AddPolicy(
                    AuthorizationPolicies.InterviewOrAdmin,
                    policy => policy.RequireAssertion(context =>
                        context.User.IsInRole(UserRole.Interviewer.ToString()) ||
                        context.User.IsInRole(UserRole.Admin.ToString()))
                );

                options.AddPolicy(
                    AuthorizationPolicies.IntervieweeOrAdmin,
                    policy => policy.RequireAssertion(context =>
                        context.User.IsInRole(UserRole.Interviewee.ToString()) ||
                        context.User.IsInRole(UserRole.Admin.ToString()))
                );
            });

            // --- CORS CONFIGURATION ---
            builder.Services.AddCors(options =>
            {
                // Allow all origin when in development
                options.AddPolicy(name: CorsPolicies.DevCorsPolicy, policy =>
                {
                    string? currentIpV4 = GetLocalIPv4();
                    policy.WithOrigins("https://localhost:5173", $"https://{currentIpV4}:5173", "http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
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

            // --- SIGNALR ---
            builder.Services.AddSignalR();

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
            }
            else
            {
                app.UseCors(CorsPolicies.ProdCorsPolicy);
            }

            app.MapHub<InterviewRoomHub>("/api/v1/hubs/interviewroom");

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        public static string? GetLocalIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(ip.Address))
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return null;
        }
    }
}
