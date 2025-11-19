using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Intervu.Infrastructure.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer;
using Intervu.Infrastructure.ExternalServices.EmailServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using PayOS;
using Intervu.Infrastructure.ExternalServices.PayOSPaymentService;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Firebase.Storage;
using FirebaseAdmin;
using Intervu.Infrastructure.ExternalServices.FirebaseStorageService;

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
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IInterviewerAvailabilitiesRepository, InterviewerAvailabilitiesRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            return services;
        }

        public static IServiceCollection AddInfrastructureExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            var firebaseSection = configuration.GetSection("Firebase");
            var bucketName = firebaseSection["StorageBucket"];
            var credentialPath = firebaseSection["CredentialPath"];
            // Temporarily disable Firebase until credentials are configured
            //var firebaseSection = configuration.GetSection("Firebase");
            //var bucketName = firebaseSection["StorageBucket"];
            //var credentialPath = firebaseSection["CredentialPath"];

            if (string.IsNullOrEmpty(credentialPath))
                throw new Exception("Firebase CredentialJson is missing in secrets.json");

            var credential = GoogleCredential.FromJson(credentialPath);

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });
            }

            services.AddSingleton(StorageClient.Create(credential));

            services.AddSingleton<string>(sp => bucketName);

            services.AddTransient<IFileService>(sp =>
            {
                var storageClient = sp.GetRequiredService<StorageClient>();
                var bucket = sp.GetRequiredService<string>();
                return new FirebaseStorageService(storageClient, bucket);
            });

            //services.AddSingleton(StorageClient.Create(credential));
            //services.AddSingleton(bucketName);
            services.AddScoped<IEmailService, ExternalServices.EmailServices.EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddSingleton<IMailService, ExternalServices.EmailService>();
            //services.AddSingleton<IMailService, ExternalServices.EmailService>();
            //services.AddTransient<IFileService, FirebaseStorageService>();
            
            // Temporary stub for IFileService - replace with Firebase when ready
            services.AddTransient<IFileService, TempFileService>();
            services.AddSingleton(sp =>
            {
                PayOSOptions? options = sp.GetRequiredService<IConfiguration>()
                   .GetSection("PayOS:Payment")
                   .Get<PayOSOptions>();

                if (options == null || string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ChecksumKey)) throw new ArgumentException("Not found PayOS config");

                return new PaymentClient(options);
            });

            services.AddSingleton(sp =>
            {
                PayOSOptions? options = sp.GetRequiredService<IConfiguration>()
                   .GetSection("PayOS:Payout")
                   .Get<PayOSOptions>();

                if (options == null || string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ChecksumKey)) throw new ArgumentException("Not found PayOS config");

                return new PayoutClient(options);
            });

            services.AddSingleton<IPaymentService>(sp =>
            {
                var paymentClient = sp.GetRequiredService<PaymentClient>();
                var payoutClient = sp.GetRequiredService<PayoutClient>();

                string returnUrl = configuration["PayOS:Payment:ReturnEndpoint"]!;
                string cancelUrl = configuration["PayOS:Payment:CancelEndpoint"]!;

                return new PayOSPaymentService(paymentClient, payoutClient, returnUrl, cancelUrl);
            });

            services.AddScoped<CodeExecutionService>();

            //Add HttpClient to call from API
            services.AddHttpClient("CodeExecutionClient", (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                string baseUrl = config["ApiClients:CodeExecution"];

                client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHostedService<InterviewRoomCache>();
            services.AddHostedService<InterviewMonitorService>();

            return services;
        }
    }
}
