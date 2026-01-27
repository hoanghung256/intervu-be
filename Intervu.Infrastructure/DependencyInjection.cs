using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Intervu.Infrastructure.ExternalServices;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Infrastructure.ExternalServices.EmailServices;
using Intervu.Application.Interfaces.ExternalServices.Email;
using PayOS;
using Intervu.Infrastructure.ExternalServices.PayOSPaymentService;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Firebase.Storage;
using FirebaseAdmin;
using Intervu.Infrastructure.ExternalServices.FirebaseStorageService;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Intervu.Infrastructure.Persistence.PostgreSQL;

namespace Intervu.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContextPool<IntervuDbContext>(options =>
            //options.UseSqlServer(configuration.GetConnectionString("SqlDefeaultConnection")));

            // PostgreSQL
            services.AddDbContextPool<IntervuPostgreDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgreSqlDefaultConnection")));

            // Register your repositories here
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInterviewRoomRepository, InterviewRoomRepository>();
            services.AddScoped<ICoachProfileRepository, CoachProfileRepository>();
            services.AddScoped<ICandidateProfileRepository, CandidateProfileRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ISkillRepository, SkillRepository>();
            //services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ICoachAvailabilitiesRepository, CoachAvailabilitiesRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IInterviewTypeRepository, InterviewTypeRepository>();

            return services;
        }

        public static IServiceCollection AddInfrastructureExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            var firebaseConfigJson = configuration["Firebase:CredentialPath"];
            var bucketName = configuration["Firebase:StorageBucket"];

            if (string.IsNullOrWhiteSpace(firebaseConfigJson))
                throw new ArgumentNullException(nameof(firebaseConfigJson), "Firebase credential JSON is missing.");

            GoogleCredential credential = GoogleCredential.FromJson(firebaseConfigJson);

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
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            //services.AddSingleton<IMailService, EmailService>();
            //services.AddSingleton<IMailService, ExternalServices.EmailService>();

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

            services.AddHostedService<InterviewRoomCacheLoader>();
            services.AddHostedService<InterviewMonitorService>();

            return services;
        }
    }
}

