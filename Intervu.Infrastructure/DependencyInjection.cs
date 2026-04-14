using Firebase.Storage;
using Polly;
using Polly.Extensions.Http;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Application.Interfaces.BackgroundJobs;
using Intervu.Application.Interfaces.ExternalServices.Email;
using Intervu.Domain.Repositories;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.Repositories;
using Intervu.Infrastructure.ExternalServices;
using Intervu.Infrastructure.ExternalServices.EmailServices;
using Intervu.Infrastructure.ExternalServices.FirebaseStorageService;
using Intervu.Infrastructure.ExternalServices.PayOSPaymentService;
using Intervu.Infrastructure.ExternalServices.Pinecone;
using Intervu.Application.Interfaces.ExternalServices.Pinecone;

using Intervu.Infrastructure.Persistence.PostgreSQL;
using Intervu.Infrastructure.BackgroundJobs;
using Intervu.Domain.Abstractions.Entity.Interfaces;
using Hangfire;
using Intervu.Application.Utils;
using Hangfire.PostgreSql;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayOS;
using System;

namespace Intervu.Infrastructure
{
    public static class DependencyInjection
    {
        private static readonly object _firebaseLock = new object();

        public static IServiceCollection AddPersistenceSqlServer(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            //services.AddDbContextPool<IntervuDbContext>(options =>
            //options.UseSqlServer(configuration.GetConnectionString("SqlDefeaultConnection")));

            // PostgreSQL
            services.AddDbContextPool<IntervuPostgreDbContext>(options =>
            {
                if (environment.IsEnvironment("Testing"))
                {
                    options
                        .UseInMemoryDatabase("Intervu_TestDb")
                        .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                }
                else
                {
                    options.UseNpgsql(configuration.GetConnectionString("PostgreSqlDefaultConnection"));
                }
            });

            if (environment.IsEnvironment("Testing"))
            {
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<IntervuPostgreDbContext>();
                    db.Database.EnsureCreated();
                }
            }

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register your repositories here
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInterviewRoomRepository, InterviewRoomRepository>();
            services.AddScoped<ICoachProfileRepository, CoachProfileRepository>();
            services.AddScoped<ICandidateProfileRepository, CandidateProfileRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ISkillRepository, SkillRepository>();
            services.AddScoped<IIndustryRepository, IndustryRepository>();
            //services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ICoachAvailabilitiesRepository, CoachAvailabilitiesRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IRescheduleRequestRepository, RescheduleRequestRepository>();
            // Assessment raw answers repo not registered — we only store processed survey snapshots
            services.AddScoped<IUserSkillAssessmentSnapshotRepository, UserSkillAssessmentSnapshotRepository>();
            services.AddScoped<IInterviewTypeRepository, InterviewTypeRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IGeneratedQuestionRepository, GeneratedQuestionRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IInterviewExperienceRepository, InterviewExperienceRepository>();
            services.AddScoped<IInterviewReportRepository, InterviewReportRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IUserQuestionLikeRepository, UserQuestionLikeRepository>();
            services.AddScoped<IUserCommentLikeRepository, UserCommentLikeRepository>();
            services.AddScoped<IQuestionReportRepository, QuestionReportRepository>();
            services.AddScoped<ICoachInterviewServiceRepository, CoachInterviewServiceRepository>();
            services.AddScoped<IBookingRequestRepository, BookingRequestRepository>();
            services.AddScoped<IInterviewRoundRepository, InterviewRoundRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IAudioChunkRepository, AudioChunkRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            return services;
        }

        public static IServiceCollection AddInfrastructureExternalServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var isTesting = environment.IsEnvironment("Testing");

            var firebaseConfigJson = configuration["Firebase:CredentialPath"];
            var bucketName = configuration["Firebase:StorageBucket"];

            if (string.IsNullOrWhiteSpace(firebaseConfigJson))
                throw new ArgumentNullException(nameof(firebaseConfigJson), "Firebase credential JSON is missing.");

            GoogleCredential credential = GoogleCredential.FromFile(firebaseConfigJson);

            lock (_firebaseLock)
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential
                    });
                }
            }

            services.AddSingleton(StorageClient.Create(credential));

            services.AddSingleton<string>(sp => bucketName);
            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

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
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PayOSPaymentService>>();

                string returnUrl = configuration["PayOS:Payment:ReturnEndpoint"]!;
                string cancelUrl = configuration["PayOS:Payment:CancelEndpoint"]!;

                return new PayOSPaymentService(paymentClient, payoutClient, returnUrl, cancelUrl, logger);
            });

            services.AddScoped<CodeExecutionService>();
            services.AddScoped<IAiService, AiService>();

            // Pinecone Services
            services.AddHttpClient<IEmbeddingService, PineconeInferenceService>();
            services.AddHttpClient<IVectorStoreService, PineconeVectorStoreService>();

            // AI Reasoning Services
            services.AddHttpClient<ExternalServices.AI.HuggingFaceReasoningService>();
            services.AddHttpClient<ExternalServices.AI.GeminiNativeReasoningService>();
            services.AddScoped<Application.Interfaces.ExternalServices.AI.ISmartSearchReasoningService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var provider = (config["ReasoningApi:Provider"] ?? "huggingface").Trim().ToLowerInvariant();
                return provider == "gemini"
                    ? sp.GetRequiredService<ExternalServices.AI.GeminiNativeReasoningService>()
                    : sp.GetRequiredService<ExternalServices.AI.HuggingFaceReasoningService>();
            });
            services.AddHttpClient<IPythonAiService, ExternalServices.AI.PythonAiService>();


            //Add HttpClient to call from API
            services.AddHttpClient("CodeExecutionClient", (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                string baseUrl = config["ApiClients:CodeExecution"];

                client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient("AiServiceClient", (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                string baseUrl = config["ApiClients:AiService"];

                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }

                client.Timeout = TimeSpan.FromSeconds(60);
            })
            .AddPolicyHandler((sp, _) => HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 2,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, attempt, _) =>
                    {
                        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("AiServiceClient");
                        logger.LogWarning(
                            "AI service call failed (attempt {Attempt}), retrying in {Delay}s. Reason: {Reason}",
                            attempt, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
                    }));

            services.AddHostedService<InterviewRoomCacheLoader>();

            if (isTesting)
            {
                services.AddScoped<IBackgroundService, NoopBackgroundService>();
            }
            else
            {
                // --- HANGFIRE JOBS ---
                services.AddScoped<HangfireJobScheduler>();
                services.AddScoped<InterviewMonitorJob>();
                services.AddScoped<IRecurringJob>(sp => sp.GetRequiredService<InterviewMonitorJob>());
                services.AddScoped<BookingExpireJob>();
                services.AddScoped<IRecurringJob>(sp => sp.GetRequiredService<BookingExpireJob>());

                // HANGFIRE
                services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(configuration.GetConnectionString("PostgreSqlDefaultConnection")));

                services.AddHangfireServer();
                services.AddScoped<IBackgroundService, HangfireBackgroundService>();
            }

            return services;
        }
    }
}
