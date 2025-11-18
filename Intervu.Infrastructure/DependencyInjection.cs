using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Intervu.Infrastructure.ExternalServices;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Google.Cloud.Storage.V1;
using Intervu.Infrastructure.ExternalServices.FirebaseStorageService;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer;

namespace Intervu.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextPool<IntervuDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlDefeaultConnection")));

            // Register your repositories here
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInterviewRoomRepository, InterviewRoomRepository>();
            services.AddScoped<IInterviewerProfileRepository, InterviewerProfileRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ISkillRepository, SkillRepository>();
            services.AddScoped<IInterviewerAvailabilitiesRepository, InterviewerAvailabilitiesRepository>();

            return services;
        }

        public static IServiceCollection AddInfrastructureExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            //var firebaseSection = configuration.GetSection("Firebase");
            //var bucketName = firebaseSection["StorageBucket"];
            //var credentialPath = firebaseSection["CredentialPath"];

            //if (string.IsNullOrEmpty(credentialPath))
            //    throw new Exception("Firebase CredentialJson is missing in secrets.json");

            //var credential = GoogleCredential.FromJson(credentialPath);

            //if (FirebaseApp.DefaultInstance == null)
            //{
            //    FirebaseApp.Create(new AppOptions
            //    {
            //        Credential = credential
            //    });
            //}

            //services.AddSingleton(StorageClient.Create(credential));
            //services.AddSingleton(bucketName);
            services.AddTransient<IMailService, EmailService>();
            //services.AddTransient<IFileService, FirebaseStorageService>();
            services.AddScoped<CodeExecutionService>();

            //Add HttpClient to call from API
            services.AddHttpClient("CodeExecutionClient", (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                string baseUrl = config["ApiClients:CodeExecution"];

                client.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }
}
