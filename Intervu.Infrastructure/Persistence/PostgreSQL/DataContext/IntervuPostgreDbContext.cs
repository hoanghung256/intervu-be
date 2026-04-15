using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using System.Text.Json;

namespace Intervu.Infrastructure.Persistence.PostgreSQL.DataContext
{
    public class IntervuPostgreDbContext : DbContext
    {
        public IntervuPostgreDbContext()
        {
        }

        public IntervuPostgreDbContext(DbContextOptions<IntervuPostgreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CandidateProfile> CandidateProfiles { get; set; }
        public DbSet<CandidateWorkExperience> CandidateWorkExperiences { get; set; }
        public DbSet<CoachProfile> CoachProfiles { get; set; }
        public DbSet<CoachWorkExperience> CoachWorkExperiences { get; set; }
        public DbSet<CandidateCertificate> CandidateCertificates { get; set; }
        public DbSet<CoachCertificate> CoachCertificates { get; set; }
        public DbSet<CoachAvailability> CoachAvailabilities { get; set; }
        public DbSet<InterviewRoom> InterviewRooms { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<InterviewBookingTransaction> InterviewBookingTransaction { get; set; }
        public DbSet<InterviewRescheduleRequest> InterviewRescheduleRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Industry> Industries { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<InterviewType> InterviewTypes { get; set; }
        public DbSet<InterviewExperience> InterviewExperiences { get; set; }
        public DbSet<InterviewReport> InterviewReports { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<GeneratedQuestion> GeneratedQuestions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<QuestionTag> QuestionTags { get; set; }
        public DbSet<QuestionCompany> QuestionCompanies { get; set; }
        public DbSet<QuestionRole> QuestionRoles { get; set; }
        public DbSet<QuestionReport> QuestionReports { get; set; }
        public DbSet<UserQuestionLike> UserQuestionLikes { get; set; }
        public DbSet<UserCommentLike> UserCommentLikes { get; set; }
        public DbSet<CoachInterviewService> CoachInterviewServices { get; set; }
        public DbSet<BookingRequest> BookingRequests { get; set; }
        public DbSet<Intervu.Domain.Entities.InterviewRound> InterviewRounds { get; set; }
        public DbSet<UserAssessmentAnswer> UserAssessmentAnswers { get; set; }
        public DbSet<UserSkillAssessmentSnapshot> UserSkillAssessments { get; set; }
        public DbSet<AudioChunk> AudioChunks { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? "Production";

                var basePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "..",
                    "Intervu.Api"
                );

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile($"appsettings.{env}.json", optional: false)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                var connectionString = configuration.GetConnectionString("PostgreSqlDefaultConnection");

                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed GUIDs
            var user1Id = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
            var user2Id = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
            var user3Id = Guid.Parse("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33");
            var user5Id = Guid.Parse("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44");
            var user6Id = Guid.Parse("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55");

            var room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");
            var CoachAvail1Id = Guid.Parse("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77");

            // New seed IDs for testing
            var roomEvaluationId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");
            var roomReportId = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
            var transactionEvaluationId = Guid.Parse("b1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5e");
            var transactionReportId = Guid.Parse("c1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5f");
            var availEvaluationId = Guid.Parse("d1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c60");
            var availReportId = Guid.Parse("e1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c61");

            // Reschedule seed data
            var roomRescheduleCreateId = Guid.Parse("b1b1b1b1-2222-4a1a-8a1a-222222222222");
            var roomRescheduleRespondId = Guid.Parse("c1c1c1c1-3333-4a1a-8a1a-333333333333");
            var availProposedCreateId = Guid.Parse("d1d1d1d1-4444-4a1a-8a1a-444444444444");
            var availProposedRespondId = Guid.Parse("e1e1e1e1-5555-4a1a-8a1a-555555555555");
            var rescheduleRequestId = Guid.Parse("f1f1f1f1-6666-4a1a-8a1a-666666666666");
            var transactionRescheduleCreateId = Guid.Parse("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a11");
            var transactionRescheduleRespondId = Guid.Parse("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a22");

            // Feedback seed data
            var feedbackTestRoomId = Guid.Parse("f1f1f1f1-7777-4a1a-8a1a-777777777777");
            var feedbackTestAvailId = Guid.Parse("f1f1f1f1-8888-4a1a-8a1a-888888888888");
            var feedbackTestTransactionId = Guid.Parse("f1f1f1f1-9999-4a1a-8a1a-999999999999");
            var feedbackUpdatePendingId = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c11");

            // Users
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(x => x.Id);
                b.Property(x => x.FullName).HasMaxLength(200);
                b.Property(x => x.Email).HasMaxLength(200).IsRequired();
                b.HasIndex(x => x.Email).IsUnique();
                b.Property(x => x.Password).HasMaxLength(500);
                b.Property(x => x.ProfilePicture).HasMaxLength(1000);
                b.Property(x => x.Role).IsRequired();
                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.SlugProfileUrl).HasMaxLength(255).IsRequired();
                b.HasIndex(x => x.SlugProfileUrl)
                    .IsUnique();
            });

            // CandidateProfile (one-to-one with User, shared PK)
            modelBuilder.Entity<CandidateProfile>(b =>
            {
                b.ToTable("CandidateProfiles");
                b.HasKey(x => x.Id);
                b.Property(x => x.CVUrl).HasMaxLength(1000);
                b.Property(x => x.PortfolioUrl).HasMaxLength(1000);
                b.Property(x => x.Bio).HasColumnType("text");
                b.Property(x => x.AIEvaluation).HasColumnName("AIEvaluation").HasColumnType("jsonb").IsRequired(false);

                var savedQuestionComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<System.Collections.Generic.List<Intervu.Domain.Entities.QuestionSnapshot>>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                    c => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<Intervu.Domain.Entities.QuestionSnapshot>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null)!);

                var stringListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<System.Collections.Generic.List<string>>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                    c => c == null ? new System.Collections.Generic.List<string>() : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null)!);

                // Saved questions stored as JSONB (nullable)
                b.Property(x => x.SavedQuestions)
                 .HasColumnName("SavedQuestions")
                 .HasColumnType("jsonb")
                 .HasConversion(
                     v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                     v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<Intervu.Domain.Entities.QuestionSnapshot>>(v, (System.Text.Json.JsonSerializerOptions?)null))
                 .IsRequired(false)
                 .Metadata.SetValueComparer(savedQuestionComparer);

                // Candidate certificates are stored as a separate table
                b.HasMany(x => x.WorkExperiences)
                 .WithOne(x => x.CandidateProfile)
                 .HasForeignKey(x => x.CandidateProfileId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Explicitly map navigation to User (like CoachProfile)
                b.HasOne(x => x.User)
                 .WithOne()
                 .HasForeignKey<CandidateProfile>(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);

                // Many-to-many: CandidateProfile <-> Skill
                b.HasMany(x => x.Skills)
                 .WithMany()
                 .UsingEntity<Dictionary<string, object>>(
                     "CandidateSkills",
                     l => l.HasOne<Skill>().WithMany().HasForeignKey("SkillsId").OnDelete(DeleteBehavior.Cascade),
                     r => r.HasOne<CandidateProfile>().WithMany().HasForeignKey("CandidateProfilesId").OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey("CandidateProfilesId", "SkillsId");
                         j.ToTable("CandidateSkills");
                     });

                // Many-to-many: CandidateProfile <-> Industry (Domain)
                b.HasMany(x => x.Industries)
                 .WithMany()
                 .UsingEntity<Dictionary<string, object>>(
                     "CandidateIndustries",
                     l => l.HasOne<Industry>().WithMany().HasForeignKey("IndustriesId").OnDelete(DeleteBehavior.Cascade),
                     r => r.HasOne<CandidateProfile>().WithMany().HasForeignKey("CandidateProfilesId").OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey("CandidateProfilesId", "IndustriesId");
                         j.ToTable("CandidateIndustries");
                     });

                b.HasMany(x => x.WorkExperiences)
                 .WithOne(x => x.CandidateProfile)
                 .HasForeignKey(x => x.CandidateProfileId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CandidateWorkExperience>(b =>
            {
                b.ToTable("CandidateWorkExperiences");
                b.HasKey(x => x.Id);
                b.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
                b.Property(x => x.StartDate).IsRequired();
                b.Property(x => x.EndDate);
                b.Property(x => x.Description).HasColumnType("text");
                b.Property(x => x.IsCurrentWorking).IsRequired();
                b.Property(x => x.IsEnded).IsRequired();

                var guidListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<System.Collections.Generic.List<System.Guid>>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                    c => c == null ? new System.Collections.Generic.List<System.Guid>() : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<System.Guid>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null)!);

                b.Property(x => x.SkillIds)
                 .HasColumnName("SkillIds")
                 .HasColumnType("jsonb")
                 .HasConversion(
                     v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                     v => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<System.Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new System.Collections.Generic.List<System.Guid>())
                 .Metadata.SetValueComparer(guidListComparer);
            });


            modelBuilder.Entity<CoachProfile>(b =>
            {
                b.ToTable("CoachProfiles");
                b.HasKey(x => x.Id);

                b.Property(x => x.PortfolioUrl).HasMaxLength(4000);
                b.Property(x => x.Bio).HasColumnType("text");
                b.Property(x => x.CurrentAmount);
                b.Property(x => x.Version).IsConcurrencyToken();
                b.Property(x => x.BankBinNumber);
                b.Property(x => x.BankAccountNumber);
                b.Property(x => x.ExperienceYears);
                b.Property(x => x.CurrentJobTitle).HasMaxLength(200);
                b.Property(x => x.Status).IsRequired();

                b.HasOne(x => x.User)
                 .WithOne()
                 .HasForeignKey<CoachProfile>(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);

                var savedQuestionComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<System.Collections.Generic.List<Intervu.Domain.Entities.QuestionSnapshot>>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                    c => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<Intervu.Domain.Entities.QuestionSnapshot>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null)!);

                var coachStringListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<System.Collections.Generic.List<string>>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                    c => c == null ? new System.Collections.Generic.List<string>() : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null)!);

                // Saved questions stored as JSONB (nullable)
                b.Property(x => x.SavedQuestions)
                 .HasColumnName("SavedQuestions")
                 .HasColumnType("jsonb")
                 .HasConversion(
                     v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                     v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<Intervu.Domain.Entities.QuestionSnapshot>>(v, (System.Text.Json.JsonSerializerOptions?)null))
                 .IsRequired(false)
                 .Metadata.SetValueComparer(savedQuestionComparer);

                // Coach certificates are stored as a separate table
                b.HasMany(x => x.WorkExperiences)
                 .WithOne(x => x.CoachProfile)
                 .HasForeignKey(x => x.CoachProfileId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.Companies)
                 .WithMany(c => c.CoachProfiles)
                 .UsingEntity<Dictionary<string, object>>(
                     "CoachCompanies",
                     l => l.HasOne<Company>().WithMany().HasForeignKey("CompaniesId").OnDelete(DeleteBehavior.Cascade),
                     r => r.HasOne<CoachProfile>().WithMany().HasForeignKey("CoachProfilesId").OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey("CoachProfilesId", "CompaniesId");
                         j.ToTable("CoachCompanies");
                     });

            modelBuilder.Entity<CandidateCertificate>(b =>
            {
                b.ToTable("CandidateCertificates");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(300).IsRequired();
                b.Property(x => x.Issuer).HasMaxLength(200);
                b.Property(x => x.IssuedAt);
                b.Property(x => x.ExpiryAt);
                b.Property(x => x.Link).HasMaxLength(1000);

                b.HasOne(x => x.CandidateProfile)
                 .WithMany(p => p.Certificates)
                 .HasForeignKey(x => x.CandidateProfileId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CoachCertificate>(b =>
            {
                b.ToTable("CoachCertificates");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(300).IsRequired();
                b.Property(x => x.Issuer).HasMaxLength(200);
                b.Property(x => x.IssuedAt);
                b.Property(x => x.ExpiryAt);
                b.Property(x => x.Link).HasMaxLength(1000);

                b.HasOne(x => x.CoachProfile)
                 .WithMany(p => p.Certificates)
                 .HasForeignKey(x => x.CoachProfileId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

                b.HasMany(x => x.Skills)
                 .WithMany(s => s.CoachProfiles)
                 .UsingEntity<Dictionary<string, object>>(
                     "CoachSkills",
                     l => l.HasOne<Skill>().WithMany().HasForeignKey("SkillsId").OnDelete(DeleteBehavior.Cascade),
                     r => r.HasOne<CoachProfile>().WithMany().HasForeignKey("CoachProfilesId").OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey("CoachProfilesId", "SkillsId");
                         j.ToTable("CoachSkills");
                     });

                b.HasMany(x => x.Industries)
                 .WithMany(i => i.CoachProfiles)
                 .UsingEntity<Dictionary<string, object>>(
                     "CoachIndustries",
                     l => l.HasOne<Industry>().WithMany().HasForeignKey("IndustriesId").OnDelete(DeleteBehavior.Cascade),
                     r => r.HasOne<CoachProfile>().WithMany().HasForeignKey("CoachProfilesId").OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey("CoachProfilesId", "IndustriesId");
                         j.ToTable("CoachIndustries");


                     });
                b.HasMany(x => x.WorkExperiences)
                 .WithOne(x => x.CoachProfile)
                 .HasForeignKey(x => x.CoachProfileId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CoachWorkExperience>(b =>
            {
                b.ToTable("CoachWorkExperiences");
                b.HasKey(x => x.Id);
                b.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
                b.Property(x => x.StartDate).IsRequired();
                b.Property(x => x.EndDate);
                b.Property(x => x.Description).HasColumnType("text");
                b.Property(x => x.IsCurrentWorking).IsRequired();
                b.Property(x => x.IsEnded).IsRequired();

                var coachGuidListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<System.Collections.Generic.List<System.Guid>>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                    c => c == null ? new System.Collections.Generic.List<System.Guid>() : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<System.Guid>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null)!);

                b.Property(x => x.SkillIds)
                 .HasColumnName("SkillIds")
                 .HasColumnType("jsonb")
                 .HasConversion(
                     v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                     v => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<System.Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new System.Collections.Generic.List<System.Guid>())
                 .Metadata.SetValueComparer(coachGuidListComparer);
            });


            modelBuilder.Entity<Industry>(b =>
            {
                b.ToTable("Industries");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(120).IsRequired();
                b.Property(x => x.Slug).HasMaxLength(150).IsRequired();
                b.HasIndex(x => x.Name).IsUnique();
                b.HasIndex(x => x.Slug).IsUnique();
            });

            // CoachAvailability (available time ranges per coach)
            modelBuilder.Entity<CoachAvailability>(b =>
            {
                b.ToTable("CoachAvailabilities");
                b.HasKey(x => x.Id);

                b.Property(x => x.StartTime).IsRequired();
                b.Property(x => x.EndTime).IsRequired();
                b.Property(x => x.Status).IsRequired();

                b.HasOne(x => x.CoachProfile)
                .WithMany()
                .HasForeignKey(x => x.CoachId)
                .HasConstraintName("FK_CoachAvailabilities_CoachProfiles_CoachId")
                .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.InterviewRound)
                .WithMany(r => r.AvailabilityBlocks)
                .HasForeignKey(x => x.InterviewRoundId)
                .IsRequired(false)
                .HasConstraintName("FK_CoachAvailabilities_InterviewRounds_InterviewRoundId")
                .OnDelete(DeleteBehavior.SetNull);
            });

            // InterviewRoom
            modelBuilder.Entity<InterviewRoom>(b =>
            {
                b.ToTable("InterviewRooms");
                b.HasKey(x => x.Id);
                b.Property(x => x.ScheduledTime);
                b.Property(x => x.DurationMinutes);
                b.Property(x => x.VideoCallRoomUrl).HasMaxLength(1000);
                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.Type).HasConversion<int>().IsRequired();

                // JSON converters for complex properties
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };

                var dictComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Dictionary<string, string>>(
                    (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                    c => c == null ? 0 : JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                    c => JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!);

                b.Property(x => x.LanguageCodes)
                    .HasColumnName("LanguageCodes")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions))
                    .HasColumnType("text")
                    .Metadata.SetValueComparer(dictComparer);

                var objArrayComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<object[]>(
                    (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                    c => c == null ? 0 : JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                    c => JsonSerializer.Deserialize<object[]>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!);

                b.Property(x => x.TestCases)
                    .HasColumnName("TestCases")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<object[]>(v, jsonOptions))
                    .HasColumnType("text")
                    .Metadata.SetValueComparer(objArrayComparer);

                b.Property(x => x.CurrentLanguage).HasColumnName("CurrentLanguage").HasMaxLength(50);
                b.Property(x => x.ProblemDescription).HasColumnName("ProblemDescription").HasColumnType("text");
                b.Property(x => x.ProblemShortName).HasColumnName("ProblemShortName").HasMaxLength(200);

                b.HasOne<CandidateProfile>()
                 .WithMany()
                  .HasForeignKey(x => x.CandidateId)
                 .HasConstraintName("FK_InterviewRooms_CandidateProfiles_CandidateId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<CoachProfile>()
                 .WithMany()
                  .HasForeignKey(x => x.CoachId)
                 .HasConstraintName("FK_InterviewRooms_CoachProfiles_CoachId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Transaction)
                 .WithMany()
                 .HasForeignKey(x => x.TransactionId)
                 .HasConstraintName("FK_InterviewRooms_InterviewBookingTransaction_TransactionId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.CurrentAvailability)
                 .WithMany()
                 .HasForeignKey(x => x.CurrentAvailabilityId)
                 .IsRequired(false)
                 .HasConstraintName("FK_InterviewRooms_CoachAvailabilities_CurrentAvailabilityId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.BookingRequest)
                 .WithMany()
                 .HasForeignKey(x => x.BookingRequestId)
                 .IsRequired(false)
                 .HasConstraintName("FK_InterviewRooms_BookingRequests_BookingRequestId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.CoachInterviewService)
                 .WithMany()
                 .HasForeignKey(x => x.CoachInterviewServiceId)
                 .IsRequired(false)
                 .HasConstraintName("FK_InterviewRooms_CoachInterviewServices_CoachInterviewServiceId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.Property(x => x.AimLevel)
                 .HasConversion<int?>()
                 .IsRequired(false);

                b.Property(x => x.RoundNumber)
                 .IsRequired(false);

                b.Property(x => x.EvaluationResultsJson)
                  .HasColumnName("EvaluationStructure")
                  .HasColumnType("jsonb");

                b.Ignore(x => x.EvaluationResults);

                b.Property(x => x.IsEvaluationCompleted);

                // Configure Transcript
                b.Property(x => x.Transcript)
                    .HasColumnType("text");

                // Configure WhiteboardElements
                b.Property(x => x.WhiteboardElements)
                    .HasColumnName("WhiteboardElements")
                    .HasColumnType("text");

                // Configure QuestionList (JSONB)
                var questionListComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<QuestionItem>>(
                    (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                    c => c == null ? 0 : JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                    c => JsonSerializer.Deserialize<List<QuestionItem>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!);

                b.Property(x => x.QuestionList)
                    .HasColumnName("QuestionList")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
                        v => v == null ? null : JsonSerializer.Deserialize<List<QuestionItem>>(v, jsonOptions))
                    .IsRequired(false)
                    .Metadata.SetValueComparer(questionListComparer);
            });

            modelBuilder.Entity<InterviewReport>(b =>
            {
                b.ToTable("InterviewRoomReports");
                b.HasKey(x => x.Id);

                b.Property(x => x.InterviewRoomId).IsRequired();
                b.Property(x => x.ReportedBy).IsRequired();
                b.Property(x => x.ReporterId).IsRequired(false);
                b.Property(x => x.Reason).HasColumnType("text").IsRequired();
                b.Property(x => x.Details).HasColumnType("text").IsRequired(false);
                b.Property(x => x.ExpectTo).HasColumnType("text").IsRequired(false);
                b.Property(x => x.Status)
                    .HasConversion<int>()
                    .IsRequired()
                    .ValueGeneratedNever();
                b.Property(x => x.AdminNote).HasColumnType("text").IsRequired(false);
                b.Property(x => x.ResolvedAt).IsRequired(false);
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt).IsRequired();

                b.HasOne(x => x.InterviewRoom)
                 .WithMany()
                 .HasForeignKey(x => x.InterviewRoomId)
                 .HasConstraintName("FK_InterviewRoomReports_InterviewRooms_InterviewRoomId")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Reporter)
                 .WithMany()
                 .HasForeignKey(x => x.ReportedBy)
                 .HasConstraintName("FK_InterviewRoomReports_Users_ReportedBy")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(x => x.InterviewRoomId);
                b.HasIndex(x => x.ReportedBy);
                b.HasIndex(x => x.Status);
            });

            // GeneratedQuestion
            modelBuilder.Entity<GeneratedQuestion>(b =>
            {
                b.ToTable("GeneratedQuestions");
                b.HasKey(x => x.Id);
                b.Property(x => x.Title).HasMaxLength(500).IsRequired();
                b.Property(x => x.Content).HasColumnType("text").IsRequired();
                b.Property(x => x.Status).HasConversion<int>().IsRequired();
                b.Property(x => x.TagIdsJson).HasColumnType("jsonb").IsRequired(false);

                b.HasOne(x => x.InterviewRoom)
                 .WithMany(r => r.GeneratedQuestions)
                 .HasForeignKey(x => x.InterviewRoomId)
                 .HasConstraintName("FK_GeneratedQuestions_InterviewRooms_InterviewRoomId")
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Feedback
            modelBuilder.Entity<Feedback>(b =>
            {
                b.ToTable("Feedbacks");
                b.HasKey(x => x.Id);
                b.Property(x => x.Rating).IsRequired();
                b.Property(x => x.Comments).HasColumnType("text");
                b.Property(x => x.AIAnalysis).HasColumnType("text");

                b.Property<Guid>("InterviewRoomId").IsRequired();

                b.HasOne(f => f.InterviewRoom)
                 .WithOne()
                 .HasForeignKey<Feedback>(f => f.InterviewRoomId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(f => f.CoachProfile)
                 .WithMany()
                 .HasForeignKey(f => f.CoachId)
                 .HasConstraintName("FK_Feedbacks_CoachProfiles_CoachId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(f => f.CandidateProfile)
                 .WithMany()
                 .HasForeignKey(f => f.CandidateId)
                 .HasConstraintName("FK_Feedbacks_CandidateProfiles_CandidateId")
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Transaction
            modelBuilder.Entity<InterviewBookingTransaction>(b =>
            {
                b.ToTable("InterviewBookingTransaction");
                b.HasKey(x => x.Id);
                b.Property(x => x.OrderCode).UseIdentityByDefaultColumn();
                b.Property(x => x.Amount).IsRequired();
                b.Property(x => x.Type).IsRequired();
                b.Property(x => x.Status).IsRequired();

                b.HasOne(x => x.BookingRequest)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.BookingRequestId)
                .IsRequired(false)
                .HasConstraintName("FK_InterviewBookingTransaction_BookingRequests_BookingRequestId")
                .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .HasConstraintName("FK_InterviewBookingTransaction_Users_UserId")
                .OnDelete(DeleteBehavior.Cascade);
            });

            // InterviewRescheduleRequest
            modelBuilder.Entity<InterviewRescheduleRequest>(b =>
            {
                b.ToTable("InterviewRescheduleRequests");
                b.HasKey(x => x.Id);

                b.Property(x => x.InterviewRoomId).IsRequired();
                b.Property(x => x.CurrentAvailabilityId).IsRequired(false);
                b.Property(x => x.ProposedAvailabilityId).IsRequired(false);
                b.Property(x => x.ProposedStartTime).IsRequired();
                b.Property(x => x.ProposedEndTime).IsRequired();
                b.Property(x => x.RequestedBy).IsRequired();
                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.ExpiresAt).IsRequired();
                b.Property(x => x.Reason).HasColumnType("text");
                b.Property(x => x.RejectionReason).HasColumnType("text");

                // Relationships
                b.HasOne(x => x.InterviewRoom)
                 .WithMany(r => r.RescheduleRequests)
                 .HasForeignKey(x => x.InterviewRoomId)
                 .HasConstraintName("FK_InterviewRescheduleRequests_InterviewRooms_InterviewRoomId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.CurrentAvailability)
                 .WithMany()
                 .HasForeignKey(x => x.CurrentAvailabilityId)
                 .IsRequired(false)
                 .HasConstraintName("FK_InterviewRescheduleRequests_CoachAvailabilities_CurrentAvailabilityId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.ProposedAvailability)
                 .WithMany()
                 .HasForeignKey(x => x.ProposedAvailabilityId)
                 .IsRequired(false)
                 .HasConstraintName("FK_InterviewRescheduleRequests_CoachAvailabilities_ProposedAvailabilityId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Requester)
                 .WithMany()
                 .HasForeignKey(x => x.RequestedBy)
                 .HasConstraintName("FK_InterviewRescheduleRequests_Users_RequestedBy")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Responder)
                 .WithMany()
                 .HasForeignKey(x => x.RespondedBy)
                 .HasConstraintName("FK_InterviewRescheduleRequests_Users_RespondedBy")
                 .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                b.HasIndex(x => x.InterviewRoomId);
                b.HasIndex(x => x.Status);
                b.HasIndex(x => x.ExpiresAt);
                b.HasIndex(x => new { x.InterviewRoomId, x.Status });
            });

            // Notification
            modelBuilder.Entity<Notification>(b =>
            {
                b.ToTable("Notifications");
                b.HasKey(x => x.Id);
                b.Property(x => x.UserId).IsRequired();
                b.Property(x => x.Type).HasConversion<int>().IsRequired();
                b.Property(x => x.Title).HasMaxLength(200).IsRequired();
                b.Property(x => x.Message).HasColumnType("text").IsRequired();
                b.Property(x => x.ActionUrl).HasMaxLength(500);
                b.Property(x => x.IsRead).IsRequired().HasDefaultValue(false);
                b.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");

                b.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .HasConstraintName("FK_Notifications_Users_UserId")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(x => new { x.UserId, x.CreatedAt })
                 .IsDescending(false, true);

                // Partial-index equivalent: fast unread count
                b.HasIndex(x => new { x.UserId, x.IsRead });
            });

            modelBuilder.Entity<Company>(b =>
            {
                b.ToTable("Companies");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(200).IsRequired();
                b.Property(x => x.LogoPath).HasMaxLength(500);
                b.Property(x => x.Website).HasMaxLength(500);
            });

            modelBuilder.Entity<Skill>(b =>
            {
                b.ToTable("Skills");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(200).IsRequired();
                b.Property(x => x.Description).HasColumnType("text");
            });

            // UserAssessmentAnswer - raw answers store
            modelBuilder.Entity<UserAssessmentAnswer>(b =>
            {
                b.ToTable("UserAssessmentAnswers");
                b.HasKey(x => x.Id);

                b.Property(x => x.UserId).IsRequired();
                b.Property(x => x.AssessmentId).IsRequired();

                b.Property(x => x.QuestionId)
                 .IsRequired()
                 .HasMaxLength(200);

                b.Property(x => x.Skill)
                 .IsRequired()
                 .HasMaxLength(200);

                b.Property(x => x.Answer)
                 .HasColumnType("text")
                 .IsRequired(false);

                b.Property(x => x.SelectedLevel)
                 .IsRequired()
                 .HasMaxLength(20);

                b.Property(x => x.Type)
                 .IsRequired()
                 .HasMaxLength(20);

                b.Property(x => x.SfiaLevel)
                 .IsRequired();

                b.Property(x => x.CreatedAt)
                 .IsRequired()
                 .HasColumnType("timestamp with time zone")
                 .HasDefaultValueSql("NOW()");

                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.AssessmentId);
                b.HasIndex(x => x.Skill);

                b.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasCheckConstraint("CK_UserAssessmentAnswers_SelectedLevel", "\"SelectedLevel\" IN ('None','Basic','Intermediate','Advanced')");
                b.HasCheckConstraint("CK_UserAssessmentAnswers_SfiaLevel", "\"SfiaLevel\" IN (0,2,3,5)");
            });

            modelBuilder.Entity<UserSkillAssessmentSnapshot>(b =>
            {
                b.ToTable("UserSkillAssessmentSnapshots");
                b.HasKey(x => x.Id);

                b.Property(x => x.UserId).IsRequired();

                b.Property(x => x.TargetJson)
                 .IsRequired()
                 .HasColumnType("jsonb");

                b.Property(x => x.CurrentJson)
                 .IsRequired()
                 .HasColumnType("jsonb");

                b.Property(x => x.GapJson)
                 .IsRequired()
                 .HasColumnType("jsonb");

                b.Property(x => x.RoadMapJson)
                 .IsRequired()
                 .HasColumnType("jsonb");

                b.Property(x => x.AnswerJson)
                 .IsRequired(false)
                 .HasColumnType("jsonb");

                b.Property(x => x.CreatedAt)
                 .IsRequired()
                 .HasColumnType("timestamp with time zone")
                 .HasDefaultValueSql("NOW()");

                b.Property(x => x.UpdatedAt)
                 .IsRequired()
                 .HasColumnType("timestamp with time zone")
                 .HasDefaultValueSql("NOW()");

                b.HasIndex(x => x.UserId)
                 .IsUnique();

                b.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PasswordResetToken>(b =>
            {
                b.ToTable("PasswordResetTokens");
                b.HasKey(x => x.Id);

                b.Property(x => x.UserId).IsRequired();

                b.Property(x => x.Token).HasMaxLength(500).IsRequired();

                b.Property(x => x.ExpiresAt).IsRequired().HasColumnType("timestamp with time zone");

                b.Property(x => x.IsUsed).IsRequired().HasDefaultValue(false);
                b.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");

                // Foreign key relationship
                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for performance
                b.HasIndex(x => x.Token);
                b.HasIndex(x => new { x.UserId, x.ExpiresAt });
            });

            modelBuilder.Entity<RefreshToken>(r =>
            {
                r.ToTable("RefreshTokens");
                r.HasKey(x => x.Id);

                r.Property(x => x.UserId).IsRequired();
                r.Property(x => x.Token).HasMaxLength(500).IsRequired();

                r.Property(x => x.ExpiresAt).IsRequired().HasColumnType("timestamp with time zone");
                r.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");

                r.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                r.HasIndex(x => x.Token);
                r.HasIndex(x => new { x.UserId, x.ExpiresAt });
            });

            modelBuilder.Entity<InterviewType>(entity =>
            {
                entity.ToTable("InterviewTypes", tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_InterviewTypes_SuggestedDurationMinutes_MultipleOf30",
                        "\"SuggestedDurationMinutes\" >= 15 AND \"SuggestedDurationMinutes\" <= 300 AND \"SuggestedDurationMinutes\" % 30 = 0");
                });

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(e => e.MinPrice)
                      .HasDefaultValue(0);

                entity.Property(e => e.MaxPrice)
                      .HasDefaultValue(0);

                entity.Property(e => e.SuggestedDurationMinutes)
                      .HasDefaultValue(60);

                entity.Property(e => e.Status)
                      .HasConversion<int>();

                entity.Property(e => e.EvaluationStructureJson)
                  .HasColumnName("EvaluationStructure")
                  .HasColumnType("jsonb");

                entity.Ignore(e => e.EvaluationStructure);
            });

            // CoachInterviewService (many-to-many with payload: Coach × InterviewType)
            modelBuilder.Entity<CoachInterviewService>(b =>
            {
                b.ToTable("CoachInterviewServices", tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_CoachInterviewServices_DurationMinutes_MultipleOf30",
                        "\"DurationMinutes\" >= 15 AND \"DurationMinutes\" <= 300 AND \"DurationMinutes\" % 30 = 0");
                });
                b.HasKey(x => x.Id);

                b.Property(x => x.Price).IsRequired();
                b.Property(x => x.DurationMinutes).IsRequired();

                b.HasOne(x => x.CoachProfile)
                 .WithMany(c => c.InterviewServices)
                 .HasForeignKey(x => x.CoachId)
                 .HasConstraintName("FK_CoachInterviewServices_CoachProfiles_CoachId")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.InterviewType)
                 .WithMany()
                 .HasForeignKey(x => x.InterviewTypeId)
                 .HasConstraintName("FK_CoachInterviewServices_InterviewTypes_InterviewTypeId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => new { x.CoachId, x.InterviewTypeId })
                 .IsUnique()
                 .HasDatabaseName("IX_CoachInterviewServices_CoachId_InterviewTypeId");
            });

            // BookingRequest (Flow B: External, Flow C: JD Multi-Round)
            modelBuilder.Entity<BookingRequest>(b =>
            {
                b.ToTable("BookingRequests");
                b.HasKey(x => x.Id);

                b.Property(x => x.Type).HasConversion<int>().IsRequired();
                b.Property(x => x.Status).HasConversion<int>().IsRequired();
                b.Property(x => x.AimLevel).HasConversion<int?>().IsRequired(false);
                b.Property(x => x.TotalAmount).IsRequired();
                b.Property(x => x.JobDescriptionUrl).HasMaxLength(1000);
                b.Property(x => x.CVUrl).HasMaxLength(1000);
                b.Property(x => x.RejectionReason).HasColumnType("text");

                b.HasOne(x => x.Candidate)
                 .WithMany()
                 .HasForeignKey(x => x.CandidateId)
                 .HasConstraintName("FK_BookingRequests_CandidateProfiles_CandidateId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Coach)
                 .WithMany()
                 .HasForeignKey(x => x.CoachId)
                 .HasConstraintName("FK_BookingRequests_CoachProfiles_CoachId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.CoachInterviewService)
                 .WithMany()
                 .HasForeignKey(x => x.CoachInterviewServiceId)
                 .IsRequired(false)
                 .HasConstraintName("FK_BookingRequests_CoachInterviewServices_CoachInterviewServiceId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => x.Status);
                b.HasIndex(x => x.CandidateId);
                b.HasIndex(x => x.CoachId);
                b.HasIndex(x => x.ExpiresAt);
            });

            // InterviewRound (multi-round for Flow C JD interviews)
            modelBuilder.Entity<Intervu.Domain.Entities.InterviewRound>(b =>
            {
                b.ToTable("InterviewRounds");
                b.HasKey(x => x.Id);

                b.Property(x => x.RoundNumber).IsRequired();
                b.Property(x => x.StartTime).IsRequired();
                b.Property(x => x.EndTime).IsRequired();
                b.Property(x => x.Price).IsRequired();

                b.HasOne(x => x.BookingRequest)
                 .WithMany(br => br.Rounds)
                 .HasForeignKey(x => x.BookingRequestId)
                 .HasConstraintName("FK_InterviewRounds_BookingRequests_BookingRequestId")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.CoachInterviewService)
                 .WithMany()
                 .HasForeignKey(x => x.CoachInterviewServiceId)
                 .HasConstraintName("FK_InterviewRounds_CoachInterviewServices_CoachInterviewServiceId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.InterviewRoom)
                 .WithMany()
                 .HasForeignKey(x => x.InterviewRoomId)
                 .IsRequired(false)
                 .HasConstraintName("FK_InterviewRounds_InterviewRooms_InterviewRoomId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => new { x.BookingRequestId, x.RoundNumber })
                 .IsUnique()
                 .HasDatabaseName("IX_InterviewRounds_BookingRequestId_RoundNumber");
            });

            // AudioChunk
            modelBuilder.Entity<AudioChunk>(b =>
            {
                b.ToTable("AudioChunks");
                b.HasKey(x => x.Id);
                b.Property(x => x.AudioData).IsRequired();
                b.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
                b.Property(x => x.RecordingSessionId).IsRequired();
                b.Property(x => x.ChunkSequenceNumber).IsRequired().HasDefaultValue(0);
                
                // Index for grouping chunks by recording session
                b.HasIndex(x => x.RecordingSessionId).HasName("IX_AudioChunks_RecordingSessionId");
                
                // Composite index for ordering chunks by session and sequence
                b.HasIndex(x => new { x.RecordingSessionId, x.ChunkSequenceNumber })
                    .HasName("IX_AudioChunks_RecordingSession_Sequence");
            });

            // AuditLog
            modelBuilder.Entity<AuditLog>(b =>
            {
                b.ToTable("AuditLogs");
                b.HasKey(x => x.Id);
                b.Property(x => x.EventType).IsRequired();
                b.Property(x => x.Timestamp).IsRequired();
                b.Property(x => x.Content).IsRequired();
                b.Property(x => x.MetaData).HasColumnType("jsonb");

                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.EventType);
                b.HasIndex(x => x.Timestamp);
            });

            modelBuilder.Entity<WithdrawalRequest>(b =>
            {
                b.ToTable("WithdrawalRequests");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).IsRequired();
                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.BankBinNumber).HasMaxLength(20);
                b.Property(x => x.BankAccountNumber).HasMaxLength(50);
                b.Property(x => x.Notes).HasMaxLength(1000);

                b.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.Status);
            });

            /// <summary>
            /// Global query filter for soft delete
            /// When querying any entity that has an "IsDeleted" property.
            /// 
            /// When you need to ignore this filter, use .IgnoreQueryFilters()
            /// 
            /// Ex:
            /// var allRooms = await _context.InterviewRooms
            ///     .IgnoreQueryFilters()
            ///     .ToListAsync();
            /// </summary>
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isDeletableEntity = entityType.FindProperty("IsDeleted");

                if (isDeletableEntity != null && isDeletableEntity.ClrType == typeof(bool))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var body = Expression.Equal(
                        Expression.Property(parameter, "IsDeleted"),
                        Expression.Constant(false)
                    );

                    var lambda = Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            // Seed data
            //var user1Id = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
            //var user2Id = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
            //var user3Id = Guid.Parse("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33");
            //var user5Id = Guid.Parse("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44");
            //var user6Id = Guid.Parse("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55");

            //var room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");
            //var CoachAvail1Id = Guid.Parse("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77");

            var room2Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a77");

            // Additional test data for reschedule functionality
            var CoachAvail2Id = Guid.Parse("aaaaaaaa-1111-4a1a-8a1a-111111111111"); // For reschedule testing

            var room3Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a88");
            var transaction4Id = Guid.Parse("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00");
            var CoachAvail3Id = Guid.Parse("aaaaaaaa-1111-4a1a-8a1a-111111111112");

            // Users
            var user1 = new User
            {
                Id = user1Id,
                FullName = "Alice Candidate",
                Email = "alice@example.com",
                Password = "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=",
                Role = UserRole.Candidate,
                ProfilePicture = null,
                Status = UserStatus.Active,
                SlugProfileUrl = "alice-candidate_1719000000001"
            };

            var user2 = new User
            {
                Id = user2Id,
                FullName = "Bob Coach",
                Email = "bob@example.com",
                Password = "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=",
                Role = UserRole.Coach,
                ProfilePicture = null,
                Status = UserStatus.Active,
                SlugProfileUrl = "bob-Coach_1719000000002"
            };

            var user3 = new User
            {
                Id = user3Id,
                FullName = "Admin",
                Email = "admin@example.com",
                Password = "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=",
                Role = UserRole.Admin,
                ProfilePicture = null,
                Status = UserStatus.Active,
                SlugProfileUrl = "admin_1719000000003"
            };

            var user5 = new User
            {
                Id = user5Id,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Password = user1.Password,
                Role = UserRole.Coach,
                ProfilePicture = null,
                Status = UserStatus.Active,
                SlugProfileUrl = "john-doe_1719000000004"
            };

            var user6 = new User
            {
                Id = user6Id,
                FullName = "Sarah Lee",
                Email = "sarah.lee@example.com",
                Password = user1.Password,
                Role = UserRole.Coach,
                ProfilePicture = null,
                Status = UserStatus.Active,
                SlugProfileUrl = "sarah-lee_1719000000005"
            };

            modelBuilder.Entity<User>().HasData(user1, user2, user3, user5, user6);

            modelBuilder.Entity<CandidateProfile>().HasData(new CandidateProfile
            {
                Id = user1Id,
                CVUrl = "https://example.com/cv-alice.pdf",
                PortfolioUrl = "https://portfolio.example.com/alice",
                Bio = "Aspiring backend developer."
            });

            modelBuilder.Entity("CandidateSkills").HasData(
                new { CandidateProfilesId = user1Id, SkillsId = Guid.Parse("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") },
                new { CandidateProfilesId = user1Id, SkillsId = Guid.Parse("02020202-0202-4202-8202-020202020202") }
            );

            modelBuilder.Entity<CoachProfile>().HasData(
            new CoachProfile
            {
                Id = user2Id,
                PortfolioUrl = "https://portfolio.example.com/bob",
                ExperienceYears = 8,
                CurrentJobTitle = "Senior Backend Engineer",
                Status = CoachProfileStatus.Enable,
                CurrentAmount = 0,
                Bio = "Senior Backend Engineer with real interview experience",
                BankBinNumber = "",
                BankAccountNumber = ""
            },
            new CoachProfile
            {
                Id = user5Id,
                PortfolioUrl = "https://portfolio.example.com/john",
                ExperienceYears = 6,
                CurrentJobTitle = "Technical Lead",
                CurrentAmount = 0,
                Bio = "Fullstack Engineer previously at Uber",
                Status = CoachProfileStatus.Enable,
                BankBinNumber = "",
                BankAccountNumber = ""
            },
            new CoachProfile
            {
                Id = user6Id,
                PortfolioUrl = "https://portfolio.example.com/sarah",
                ExperienceYears = 7,
                CurrentJobTitle = "Senior Frontend Engineer",
                CurrentAmount = 0,
                Bio = "Senior Frontend Engineer focusing on UI/UX interviews",
                Status = CoachProfileStatus.Enable,
                BankBinNumber = "",
                BankAccountNumber = ""
            }
            );

            modelBuilder.Entity<CoachAvailability>().HasData(
                // Current availability for room1 (already booked by Alice)
                new CoachAvailability
                {
                    Id = CoachAvail1Id,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 2, 10, 9, 0, 0), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 2, 10, 12, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                },
                // Another availability range
                new CoachAvailability
                {
                    Id = CoachAvail2Id,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 3, 15, 14, 0, 0), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 3, 15, 17, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                },
                new CoachAvailability
                {
                    Id = CoachAvail3Id,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 4, 1, 10, 0, 0), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 4, 1, 11, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                },
                new CoachAvailability
                {
                    Id = availEvaluationId,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 5, 1, 9, 0, 0), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 5, 1, 10, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                },
                new CoachAvailability
                {
                    Id = availReportId,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 5, 2, 9, 0, 0), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 5, 2, 10, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                },
                new CoachAvailability
                {
                    Id = availProposedCreateId,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 5, 3, 9, 0, 0), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 5, 3, 10, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                },
                new CoachAvailability
                {
                    Id = availProposedRespondId,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 5, 4, 9, 0, 0), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 5, 4, 10, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                },
                new CoachAvailability
                {
                    Id = feedbackTestAvailId,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 5, 5, 9, 0, 0), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 5, 5, 10, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                }
            );

            // Seed transactions for testing
            var transaction1Id = Guid.Parse("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88");
            var transaction2Id = Guid.Parse("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99");
            var transaction3Id = Guid.Parse("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99");

            modelBuilder.Entity<InterviewBookingTransaction>().HasData(
                new InterviewBookingTransaction
                {
                    Id = transaction1Id,
                    UserId = user1Id,
                    Amount = 1000,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = transaction2Id,
                    UserId = user2Id,
                    Amount = 500,
                    Type = TransactionType.Payout,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = transaction3Id,
                    UserId = user1Id,
                    Amount = 1500,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = transaction4Id,
                    UserId = user1Id,
                    Amount = 2000,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = transactionEvaluationId,
                    UserId = user1Id,
                    Amount = 2000,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = transactionReportId,
                    UserId = user1Id,
                    Amount = 2000,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = transactionRescheduleCreateId,
                    UserId = user1Id,
                    Amount = 2000,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = transactionRescheduleRespondId,
                    UserId = user1Id,
                    Amount = 2000,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = feedbackTestTransactionId,
                    UserId = user1Id,
                    Amount = 2000,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                }
            );

            modelBuilder.Entity<InterviewRoom>().HasData(
                new InterviewRoom
                {
                    Id = room1Id,
                    CandidateId = user1Id,
                    CoachId = user2Id,
                    TransactionId = transaction1Id,
                    CurrentAvailabilityId = CoachAvail1Id, // Link to current availability
                    ScheduledTime = DateTime.SpecifyKind(new DateTime(2026, 2, 10, 9, 0, 0), DateTimeKind.Utc),
                    DurationMinutes = 60,
                    VideoCallRoomUrl = "https://meet.example/room1",
                    Status = InterviewRoomStatus.Scheduled,
                    RescheduleAttemptCount = 0,
                    Type = InterviewRoomType.Normal
                },
                new InterviewRoom
                {
                    Id = room2Id,
                    CandidateId = user1Id,
                    CoachId = null,
                    TransactionId = transaction3Id,
                    CurrentAvailabilityId = CoachAvail2Id,
                    ScheduledTime = DateTime.SpecifyKind(new DateTime(2026, 3, 15, 14, 30, 0), DateTimeKind.Utc),
                    DurationMinutes = 60,
                    VideoCallRoomUrl = "https://meet.example/room-ai",
                    Status = InterviewRoomStatus.Ongoing,
                    RescheduleAttemptCount = 0,
                    Type = InterviewRoomType.WithAI
                },
                new InterviewRoom
                {
                    Id = room3Id,
                    CandidateId = user1Id,
                    CoachId = user2Id,
                    TransactionId = transaction4Id,
                    CurrentAvailabilityId = CoachAvail3Id,
                    ScheduledTime = DateTime.SpecifyKind(new DateTime(2026, 4, 1, 10, 0, 0), DateTimeKind.Utc),
                    DurationMinutes = 60,
                    VideoCallRoomUrl = "https://meet.example/room3",
                    Status = InterviewRoomStatus.Ongoing,
                    RescheduleAttemptCount = 0,
                    Type = InterviewRoomType.Normal
                },
                new InterviewRoom
                {
                    Id = roomEvaluationId,
                    CandidateId = user1Id,
                    CoachId = user2Id,
                    TransactionId = transactionEvaluationId,
                    CurrentAvailabilityId = availEvaluationId,
                    ScheduledTime = DateTime.SpecifyKind(new DateTime(2026, 5, 1, 9, 0, 0), DateTimeKind.Utc),
                    DurationMinutes = 60,
                    VideoCallRoomUrl = "https://meet.example/room-eval",
                    Status = InterviewRoomStatus.Ongoing,
                    RescheduleAttemptCount = 0,
                    Type = InterviewRoomType.Normal
                },
                new InterviewRoom
                {
                    Id = roomReportId,
                    CandidateId = user1Id,
                    CoachId = user2Id,
                    TransactionId = transactionReportId,
                    CurrentAvailabilityId = availReportId,
                    ScheduledTime = DateTime.SpecifyKind(new DateTime(2026, 5, 2, 9, 0, 0), DateTimeKind.Utc),
                    DurationMinutes = 60,
                    VideoCallRoomUrl = "https://meet.example/room-report",
                    Status = InterviewRoomStatus.Completed,
                    RescheduleAttemptCount = 0,
                    Type = InterviewRoomType.Normal
                },
                new InterviewRoom
                {
                    Id = roomRescheduleCreateId,
                    CandidateId = user1Id,
                    CoachId = user2Id,
                    TransactionId = transactionRescheduleCreateId,
                    CurrentAvailabilityId = CoachAvail1Id,
                    ScheduledTime = DateTime.SpecifyKind(new DateTime(2026, 5, 10, 9, 0, 0), DateTimeKind.Utc),
                    DurationMinutes = 60,
                    VideoCallRoomUrl = "https://meet.example/room-resch-create",
                    Status = InterviewRoomStatus.Scheduled,
                    RescheduleAttemptCount = 0,
                    Type = InterviewRoomType.Normal
                },
                new InterviewRoom
                {
                    Id = roomRescheduleRespondId,
                    CandidateId = user1Id,
                    CoachId = user2Id,
                    TransactionId = transactionRescheduleRespondId,
                    CurrentAvailabilityId = CoachAvail1Id,
                    ScheduledTime = DateTime.SpecifyKind(new DateTime(2026, 5, 12, 9, 0, 0), DateTimeKind.Utc),
                    DurationMinutes = 60,
                    VideoCallRoomUrl = "https://meet.example/room-resch-respond",
                    Status = InterviewRoomStatus.Scheduled,
                    RescheduleAttemptCount = 0,
                    Type = InterviewRoomType.Normal
                }
            );

            modelBuilder.Entity<InterviewRescheduleRequest>().HasData(
                new InterviewRescheduleRequest
                {
                    Id = rescheduleRequestId,
                    InterviewRoomId = roomRescheduleRespondId,
                    CurrentAvailabilityId = CoachAvail1Id,
                    ProposedAvailabilityId = availProposedRespondId,
                    RequestedBy = user1Id,
                    Status = RescheduleRequestStatus.Pending,
                    ExpiresAt = DateTime.SpecifyKind(new DateTime(2026, 6, 1, 0, 0, 0), DateTimeKind.Utc),
                    Reason = "Seed reason for testing response"
                }
            );

            modelBuilder.Entity<Feedback>().HasData(
                new Feedback
                {
                Id = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10"),
                    CoachId = user2Id,
                    CandidateId = user1Id,
                    InterviewRoomId = room1Id,
                    Rating = 5,
                    Comments = "Great answers and communication.",
                    AIAnalysis = "{}"
                },
                new Feedback
                {
                    Id = feedbackUpdatePendingId,
                    CoachId = user2Id,
                    CandidateId = user1Id,
                    InterviewRoomId = feedbackTestRoomId,
                    Rating = 0,
                    Comments = "",
                    AIAnalysis = "{}"
                }
            );

            modelBuilder.Entity<Notification>().HasData(new Notification
            {
                Id = Guid.Parse("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20"),
                UserId = user1Id,
                Type = Intervu.Domain.Entities.Constants.NotificationType.SystemAnnouncement,
                Title = "Welcome",
                Message = "Welcome to Intervu platform",
                ActionUrl = null,
                ReferenceId = null,
                IsRead = false,
                CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 10, 1), DateTimeKind.Utc)
            });

            modelBuilder.Entity<Company>().HasData(
                new Company { Id = Guid.Parse("11111111-1111-4111-8111-111111111111"), Name = "Google", Website = "https://google.com", LogoPath = "logos/google.png" },
                new Company { Id = Guid.Parse("22222222-2222-4222-8222-222222222222"), Name = "Meta", Website = "https://meta.com", LogoPath = "logos/meta.png" },
                new Company { Id = Guid.Parse("33333333-3333-4333-8333-333333333333"), Name = "Amazon", Website = "https://amazon.com", LogoPath = "logos/amazon.png" },
                new Company { Id = Guid.Parse("44444444-4444-4444-8444-444444444444"), Name = "Microsoft", Website = "https://microsoft.com", LogoPath = "logos/microsoft.png" },
                new Company { Id = Guid.Parse("55555555-5555-4555-8555-555555555555"), Name = "Netflix", Website = "https://netflix.com", LogoPath = "logos/netflix.png" },
                new Company { Id = Guid.Parse("66666666-6666-4666-8666-666666666666"), Name = "TikTok", Website = "https://tiktok.com", LogoPath = "logos/tiktok.png" },
                new Company { Id = Guid.Parse("77777777-7777-4777-8777-777777777777"), Name = "Apple", Website = "https://apple.com", LogoPath = "logos/apple.png" },
                new Company { Id = Guid.Parse("88888888-8888-4888-8888-888888888888"), Name = "Uber", Website = "https://uber.com", LogoPath = "logos/uber.png" },
                new Company { Id = Guid.Parse("99999999-9999-4999-8999-999999999999"), Name = "Spotify", Website = "https://spotify.com", LogoPath = "logos/spotify.png" },
                new Company { Id = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"), Name = "Stripe", Website = "https://stripe.com", LogoPath = "logos/stripe.png" },
                new Company { Id = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), Name = "Shopee", Website = "https://shopee.com", LogoPath = "logos/shopee.png" }
            );

            modelBuilder.Entity<Skill>().HasData(
                new Skill { Id = Guid.Parse("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1"), Name = "C#" },
                new Skill { Id = Guid.Parse("c2c2c2c2-c2c2-42c2-82c2-c2c2c2c2c2c2"), Name = "Java" },
                new Skill { Id = Guid.Parse("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3"), Name = "JavaScript" },
                new Skill { Id = Guid.Parse("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4"), Name = "TypeScript" },
                new Skill { Id = Guid.Parse("f5f5f5f5-f5f5-45f5-85f5-f5f5f5f5f5f5"), Name = "React" },
                new Skill { Id = Guid.Parse("01010101-0101-4101-8101-010101010101"), Name = "Node.js" },
                new Skill { Id = Guid.Parse("02020202-0202-4202-8202-020202020202"), Name = "SQL" },
                new Skill { Id = Guid.Parse("03030303-0303-4303-8303-030303030303"), Name = "MongoDB" },
                new Skill { Id = Guid.Parse("04040404-0404-4404-8404-040404040404"), Name = "AWS" },
                new Skill { Id = Guid.Parse("05050505-0505-4505-8505-050505050505"), Name = "Azure" },
                new Skill { Id = Guid.Parse("06060606-0606-4606-8606-060606060606"), Name = "System Design" },
                new Skill { Id = Guid.Parse("07070707-0707-4707-8707-070707070707"), Name = "Microservices" },
                new Skill { Id = Guid.Parse("08080808-0808-4808-8808-080808080808"), Name = "Docker" },
                new Skill { Id = Guid.Parse("09090909-0909-4909-8909-090909090909"), Name = "Kubernetes" },
                new Skill { Id = Guid.Parse("0a0a0a0a-0a0a-4a0a-8a0a-0a0a0a0a0a0a"), Name = "Machine Learning" }
            );

            modelBuilder.Entity<Industry>().HasData(
                new Industry { Id = Guid.Parse("11110000-0000-4000-8000-000000000001"), Name = "Fintech", Slug = "fintech" },
                new Industry { Id = Guid.Parse("11110000-0000-4000-8000-000000000002"), Name = "E-commerce", Slug = "e-commerce" },
                new Industry { Id = Guid.Parse("11110000-0000-4000-8000-000000000003"), Name = "EdTech", Slug = "edtech" },
                new Industry { Id = Guid.Parse("11110000-0000-4000-8000-000000000004"), Name = "Blockchain", Slug = "blockchain" },
                new Industry { Id = Guid.Parse("11110000-0000-4000-8000-000000000005"), Name = "HealthTech", Slug = "healthtech" },
                new Industry { Id = Guid.Parse("11110000-0000-4000-8000-000000000006"), Name = "SaaS", Slug = "saas" },
                new Industry { Id = Guid.Parse("11110000-0000-4000-8000-000000000007"), Name = "AI/ML", Slug = "ai-ml" },
                new Industry { Id = Guid.Parse("11110000-0000-4000-8000-000000000008"), Name = "GameDev", Slug = "gamedev" }
            );

            // Bob (user2Id)
            modelBuilder.Entity("CoachCompanies").HasData(
                new { CoachProfilesId = user2Id, CompaniesId = Guid.Parse("11111111-1111-4111-8111-111111111111") }, // Google
                new { CoachProfilesId = user2Id, CompaniesId = Guid.Parse("44444444-4444-4444-8444-444444444444") }, // Microsoft
                new { CoachProfilesId = user2Id, CompaniesId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa") } // Stripe
            );

            // John (user5Id)
            modelBuilder.Entity("CoachCompanies").HasData(
                new { CoachProfilesId = user5Id, CompaniesId = Guid.Parse("88888888-8888-4888-8888-888888888888") }, // Uber
                new { CoachProfilesId = user5Id, CompaniesId = Guid.Parse("33333333-3333-4333-8333-333333333333") }, // Amazon
                new { CoachProfilesId = user5Id, CompaniesId = Guid.Parse("66666666-6666-4666-8666-666666666666") }  // TikTok
            );

            // Sarah (user6Id)
            modelBuilder.Entity("CoachCompanies").HasData(
                new { CoachProfilesId = user6Id, CompaniesId = Guid.Parse("77777777-7777-4777-8777-777777777777") }, // Apple
                new { CoachProfilesId = user6Id, CompaniesId = Guid.Parse("99999999-9999-4999-8999-999999999999") }, // Spotify
                new { CoachProfilesId = user6Id, CompaniesId = Guid.Parse("22222222-2222-4222-8222-222222222222") }  // Meta
            );

            // Bob (backend)
            modelBuilder.Entity("CoachSkills").HasData(
                new { CoachProfilesId = user2Id, SkillsId = Guid.Parse("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") },
                new { CoachProfilesId = user2Id, SkillsId = Guid.Parse("02020202-0202-4202-8202-020202020202") },
                new { CoachProfilesId = user2Id, SkillsId = Guid.Parse("06060606-0606-4606-8606-060606060606") },
                new { CoachProfilesId = user2Id, SkillsId = Guid.Parse("07070707-0707-4707-8707-070707070707") },
                new { CoachProfilesId = user2Id, SkillsId = Guid.Parse("08080808-0808-4808-8808-080808080808") }
            );

            // John (fullstack)
            modelBuilder.Entity("CoachSkills").HasData(
                new { CoachProfilesId = user5Id, SkillsId = Guid.Parse("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                new { CoachProfilesId = user5Id, SkillsId = Guid.Parse("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                new { CoachProfilesId = user5Id, SkillsId = Guid.Parse("07070707-0707-4707-8707-070707070707") },
                new { CoachProfilesId = user5Id, SkillsId = Guid.Parse("04040404-0404-4404-8404-040404040404") },
                new { CoachProfilesId = user5Id, SkillsId = Guid.Parse("09090909-0909-4909-8909-090909090909") }
            );

            // Sarah (frontend + ML)
            modelBuilder.Entity("CoachSkills").HasData(
                new { CoachProfilesId = user6Id, SkillsId = Guid.Parse("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                new { CoachProfilesId = user6Id, SkillsId = Guid.Parse("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                new { CoachProfilesId = user6Id, SkillsId = Guid.Parse("f5f5f5f5-f5f5-45f5-85f5-f5f5f5f5f5f5") },
                new { CoachProfilesId = user6Id, SkillsId = Guid.Parse("0a0a0a0a-0a0a-4a0a-8a0a-0a0a0a0a0a0a") }
            );

            modelBuilder.Entity("CoachIndustries").HasData(
                new { CoachProfilesId = user2Id, IndustriesId = Guid.Parse("11110000-0000-4000-8000-000000000001") },
                new { CoachProfilesId = user2Id, IndustriesId = Guid.Parse("11110000-0000-4000-8000-000000000006") },
                new { CoachProfilesId = user5Id, IndustriesId = Guid.Parse("11110000-0000-4000-8000-000000000002") },
                new { CoachProfilesId = user5Id, IndustriesId = Guid.Parse("11110000-0000-4000-8000-000000000007") },
                new { CoachProfilesId = user6Id, IndustriesId = Guid.Parse("11110000-0000-4000-8000-000000000003") },
                new { CoachProfilesId = user6Id, IndustriesId = Guid.Parse("11110000-0000-4000-8000-000000000008") }
            );

            // InterviewExperience
            modelBuilder.Entity<InterviewExperience>(b =>
            {
                b.ToTable("InterviewExperiences");
                b.HasKey(x => x.Id);
                b.Property(x => x.CompanyId).IsRequired();
                b.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.Property(x => x.Role).IsRequired().HasMaxLength(300);
                b.Property(x => x.Level).HasConversion<int>();
                b.Property(x => x.LastRoundCompleted).IsRequired().HasMaxLength(200);
                b.Property(x => x.InterviewProcess).IsRequired().HasColumnType("text");
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt).IsRequired();

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.Questions)
                    .WithOne(x => x.InterviewExperience)
                    .HasForeignKey(x => x.InterviewExperienceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Question (normalized – supports M:M with companies, roles, tags, categories)
            modelBuilder.Entity<Question>(b =>
            {
                b.ToTable("Questions");
                b.HasKey(x => x.Id);
                b.Property(x => x.Title).IsRequired().HasMaxLength(500);
                b.Property(x => x.Content).IsRequired().HasColumnType("text");
                b.Property(x => x.Level).HasConversion<int>();
                b.Property(x => x.Round).HasConversion<int>();
                b.Property(x => x.Status).HasConversion<int>().HasDefaultValue(QuestionStatus.Approved);
                b.Property(x => x.ViewCount).HasDefaultValue(0);
                b.Property(x => x.SaveCount).HasDefaultValue(0);
                b.Property(x => x.Vote).HasDefaultValue(0);
                b.Property(x => x.IsHot).HasDefaultValue(false);
                b.Property(x => x.Category).HasConversion<int>();
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt).IsRequired();

                b.HasOne(x => x.Author)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.Comments)
                    .WithOne(c => c.Question)
                    .HasForeignKey(c => c.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.Reports)
                    .WithOne(r => r.Question)
                    .HasForeignKey(r => r.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for optimized filtering
                b.HasIndex(x => x.CreatedAt);
                b.HasIndex(x => x.ViewCount);
                b.HasIndex(x => x.Level);
                b.HasIndex(x => x.IsHot);
                b.HasIndex(x => x.Status);
                b.HasIndex(x => x.Category);
            });

            // Tag
            modelBuilder.Entity<Tag>(b =>
            {
                b.ToTable("Tags");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired().HasMaxLength(100);
                b.Property(x => x.Description).HasMaxLength(500);
                b.HasIndex(x => x.Name).IsUnique();
            });

            // QuestionTag (M:M join)
            modelBuilder.Entity<QuestionTag>(b =>
            {
                b.ToTable("QuestionTags");
                b.HasKey(x => new { x.QuestionId, x.TagId });
                b.HasOne(x => x.Question).WithMany(q => q.QuestionTags).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Tag).WithMany(t => t.QuestionTags).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
            });

            // QuestionCompany (M:M join – "asked at" companies)
            modelBuilder.Entity<QuestionCompany>(b =>
            {
                b.ToTable("QuestionCompanies");
                b.HasKey(x => new { x.QuestionId, x.CompanyId });
                b.HasOne(x => x.Question).WithMany(q => q.QuestionCompanies).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Company).WithMany(c => c.QuestionCompanies).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            });

            // QuestionRole (M:M join – relevant roles)
            modelBuilder.Entity<QuestionRole>(b =>
            {
                b.ToTable("QuestionRoles");
                b.HasKey(x => new { x.QuestionId, x.Role });
                b.Property(x => x.Role).HasConversion<int>();
                b.HasOne(x => x.Question).WithMany(q => q.QuestionRoles).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => x.Role);
            });

            // Comment (belongs to a Question; legacy discussion comments)
            modelBuilder.Entity<Comment>(b =>
            {
                b.ToTable("Comments");
                b.HasKey(x => x.Id);
                b.Property(x => x.Content).IsRequired().HasColumnType("text");
                b.Property(x => x.Vote).HasDefaultValue(0);
                b.Property(x => x.IsAnswer).HasDefaultValue(false);
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdateAt).IsRequired();
                b.Property(x => x.CreateBy).IsRequired();
                b.Property(x => x.UpdateBy).IsRequired();
                b.HasIndex(x => x.QuestionId);
            });

            modelBuilder.Entity<QuestionReport>(b =>
            {
                b.ToTable("QuestionReports");
                b.HasKey(x => x.Id);
                b.Property(x => x.QuestionId).IsRequired();
                b.Property(x => x.ReportedBy).IsRequired();
                b.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
                b.Property(x => x.Status).HasConversion<int>().HasDefaultValue(QuestionReportStatus.Pending);
                b.Property(x => x.CreatedAt).IsRequired();
                b.Property(x => x.UpdatedAt).IsRequired();

                b.HasOne(x => x.Question)
                    .WithMany(q => q.Reports)
                    .HasForeignKey(x => x.QuestionId)
                    .HasConstraintName("FK_QuestionReports_Questions_QuestionId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Reporter)
                    .WithMany()
                    .HasForeignKey(x => x.ReportedBy)
                    .HasConstraintName("FK_QuestionReports_Users_ReportedBy")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(x => x.QuestionId);
                b.HasIndex(x => x.ReportedBy);
                b.HasIndex(x => x.Status);
            });

            // UserQuestionLike (tracks which user liked which question)
            modelBuilder.Entity<UserQuestionLike>(b =>
            {
                b.ToTable("UserQuestionLikes");
                b.HasKey(x => new { x.UserId, x.QuestionId });
                b.Property(x => x.CreatedAt).IsRequired();

                b.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .HasConstraintName("FK_UserQuestionLikes_Users_UserId")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Question)
                 .WithMany()
                 .HasForeignKey(x => x.QuestionId)
                 .HasConstraintName("FK_UserQuestionLikes_Questions_QuestionId")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.QuestionId);
            });

            // UserCommentLike (tracks which user liked which comment)
            modelBuilder.Entity<UserCommentLike>(b =>
            {
                b.ToTable("UserCommentLikes");
                b.HasKey(x => new { x.UserId, x.CommentId });
                b.Property(x => x.CreatedAt).IsRequired();

                b.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .HasConstraintName("FK_UserCommentLikes_Users_UserId")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Comment)
                 .WithMany()
                 .HasForeignKey(x => x.CommentId)
                 .HasConstraintName("FK_UserCommentLikes_Comments_CommentId")
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(x => x.UserId);
                b.HasIndex(x => x.CommentId);
            });

            // ===================== SEED DATA – Tags =====================
            var tagAI = Guid.Parse("aa000001-0000-4000-8000-000000000001");
            var tagSQL = Guid.Parse("aa000002-0000-4000-8000-000000000002");
            var tagSysDes = Guid.Parse("aa000003-0000-4000-8000-000000000003");
            var tagProdStrat = Guid.Parse("aa000004-0000-4000-8000-000000000004");
            var tagBackend = Guid.Parse("aa000005-0000-4000-8000-000000000005");
            var tagGenAI = Guid.Parse("aa000006-0000-4000-8000-000000000006");
            var tagAlgo = Guid.Parse("aa000007-0000-4000-8000-000000000007");
            var tagFrontend = Guid.Parse("aa000008-0000-4000-8000-000000000008");
            var tagBehavior = Guid.Parse("aa000009-0000-4000-8000-000000000009");
            var tagData = Guid.Parse("aa00000a-0000-4000-8000-00000000000a");

            modelBuilder.Entity<Tag>().HasData(
                new Tag { Id = tagAI, Name = "AI", Description = "Artificial Intelligence & Machine Learning" },
                new Tag { Id = tagSQL, Name = "SQL", Description = "SQL & Database querying" },
                new Tag { Id = tagSysDes, Name = "System Design", Description = "Distributed systems & architecture design" },
                new Tag { Id = tagProdStrat, Name = "Product Strategy", Description = "Product management & strategy" },
                new Tag { Id = tagBackend, Name = "Backend", Description = "Backend engineering & APIs" },
                new Tag { Id = tagGenAI, Name = "GenAI", Description = "Generative AI, LLMs, prompt engineering" },
                new Tag { Id = tagAlgo, Name = "Algorithms", Description = "Data structures & algorithms" },
                new Tag { Id = tagFrontend, Name = "Frontend", Description = "Frontend & UI engineering" },
                new Tag { Id = tagBehavior, Name = "Behavioral", Description = "Behavioral & leadership questions" },
                new Tag { Id = tagData, Name = "Data Engineering", Description = "Data pipelines, ETL, big data" }
            );

            // ===================== SEED DATA – InterviewExperiences (kept) =====================
            var exp1Id = Guid.Parse("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d");
            var exp2Id = Guid.Parse("b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e");
            var exp3Id = Guid.Parse("c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f");

            modelBuilder.Entity<InterviewExperience>().HasData(
                new InterviewExperience
                {
                    Id = exp1Id,
                    CompanyId = Guid.Parse("11111111-1111-4111-8111-111111111111"),
                    Role = "Software Engineer",
                    Level = ExperienceLevel.Senior,
                    LastRoundCompleted = "Onsite",
                    InterviewProcess = "Phone screen → 2 technical rounds → system design → behavioral",
                    IsInterestedInContact = true,
                    CreatedBy = user1Id,
                    UpdatedBy = user1Id,
                    CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new InterviewExperience
                {
                    Id = exp2Id,
                    CompanyId = Guid.Parse("22222222-2222-4222-8222-222222222222"),
                    Role = "Frontend Developer",
                    Level = ExperienceLevel.Middle,
                    LastRoundCompleted = "System Design",
                    InterviewProcess = "Online assessment → coding interview → system design",
                    IsInterestedInContact = false,
                    CreatedBy = user1Id,
                    UpdatedBy = user1Id,
                    CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new InterviewExperience
                {
                    Id = exp3Id,
                    CompanyId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
                    Role = "Backend Engineer",
                    Level = ExperienceLevel.Junior,
                    LastRoundCompleted = "Technical",
                    InterviewProcess = "CV screening → HR call → technical interview with coding challenge",
                    IsInterestedInContact = true,
                    CreatedBy = user3Id,
                    UpdatedBy = user3Id,
                    CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ===================== SEED DATA – Questions (normalized) =====================
            // Company IDs (reusing existing seed)
            var googleId = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var metaId = Guid.Parse("22222222-2222-4222-8222-222222222222");
            var amazonId = Guid.Parse("33333333-3333-4333-8333-333333333333");
            var microsoftId = Guid.Parse("44444444-4444-4444-8444-444444444444");
            var appleId = Guid.Parse("55555555-5555-4555-8555-555555555555");
            var netflixId = Guid.Parse("66666666-6666-4666-8666-666666666666");
            var stripeId = Guid.Parse("99999999-9999-4999-8999-999999999999");

            var q1 = Guid.Parse("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a");
            var q2 = Guid.Parse("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b");
            var q3 = Guid.Parse("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c");
            var q4 = Guid.Parse("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d");
            var q5 = Guid.Parse("bb000001-0000-4000-8000-000000000001");
            var q6 = Guid.Parse("bb000002-0000-4000-8000-000000000002");
            var q7 = Guid.Parse("bb000003-0000-4000-8000-000000000003");
            var q8 = Guid.Parse("bb000004-0000-4000-8000-000000000004");
            var q9 = Guid.Parse("bb000005-0000-4000-8000-000000000005");
            var q10 = Guid.Parse("bb000006-0000-4000-8000-000000000006");
            var q11 = Guid.Parse("bb000007-0000-4000-8000-000000000007");
            var q12 = Guid.Parse("bb000008-0000-4000-8000-000000000008");
            var q13 = Guid.Parse("bb000009-0000-4000-8000-000000000009");
            var q14 = Guid.Parse("bb00000a-0000-4000-8000-00000000000a");
            var q15 = Guid.Parse("bb00000b-0000-4000-8000-00000000000b");
            var q16 = Guid.Parse("bb00000c-0000-4000-8000-00000000000c");

            modelBuilder.Entity<Question>().HasData(
                // ── AI / GenAI questions ──
                new { Id = q1, Title = "Longest Substring Without Repeating Characters", Content = "Find the longest substring without repeating characters. Explain your approach and time complexity.", InterviewExperienceId = (Guid?)exp1Id, Level = ExperienceLevel.Senior, Round = Intervu.Domain.Entities.Constants.QuestionConstants.InterviewRound.TechnicalScreen, Status = QuestionStatus.Approved, Category = QuestionCategory.Coding, ViewCount = 0, SaveCount = 0, Vote = 0, IsHot = true, CreatedBy = (Guid?)user1Id, CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc) },

                // ── Backend Engineering questions ──
                new { Id = q2, Title = "Design a URL Shortener like bit.ly", Content = "Design a URL shortener service like bit.ly. Discuss hashing strategy, data storage, redirect flow, analytics, and scaling.", InterviewExperienceId = (Guid?)exp1Id, Level = ExperienceLevel.Senior, Round = Intervu.Domain.Entities.Constants.QuestionConstants.InterviewRound.SystemDesignRound, Status = QuestionStatus.Approved, Category = QuestionCategory.SystemDesign, ViewCount = 0, SaveCount = 0, Vote = 0, IsHot = true, CreatedBy = (Guid?)user1Id, CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = q3, Title = "Explain == vs === in JavaScript", Content = "Explain the difference between == and === in JavaScript. Give examples where they produce different results.", InterviewExperienceId = (Guid?)exp2Id, Level = ExperienceLevel.Middle, Round = Intervu.Domain.Entities.Constants.QuestionConstants.InterviewRound.TechnicalScreen, Status = QuestionStatus.Approved, Category = QuestionCategory.Technical, ViewCount = 0, SaveCount = 0, Vote = 0, IsHot = false, CreatedBy = (Guid?)user1Id, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = q4, Title = "Reverse a Linked List", Content = "Reverse a singly linked list. Provide both iterative and recursive solutions with time/space complexity analysis.", InterviewExperienceId = (Guid?)exp3Id, Level = ExperienceLevel.Junior, Round = Intervu.Domain.Entities.Constants.QuestionConstants.InterviewRound.CodingChallenge, Status = QuestionStatus.Approved, Category = QuestionCategory.Coding, ViewCount = 0, SaveCount = 0, Vote = 0, IsHot = false, CreatedBy = (Guid?)user3Id, CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // ===================== SEED DATA – QuestionCompany (asked at) =====================
            modelBuilder.Entity<QuestionCompany>().HasData(
                // Q1: asked at Google
                new { QuestionId = q1, CompanyId = googleId },
                // Q2: asked at Netflix
                new { QuestionId = q2, CompanyId = netflixId },
                // Q3: asked at Meta
                new { QuestionId = q3, CompanyId = metaId },
                // Q4: asked at Microsoft
                new { QuestionId = q4, CompanyId = microsoftId }
            );

            // ===================== SEED DATA – QuestionRole =====================
            modelBuilder.Entity<QuestionRole>().HasData(
                new { QuestionId = q1, Role = Role.SoftwareEngineer },
                new { QuestionId = q1, Role = Role.BackendEngineer },
                new { QuestionId = q2, Role = Role.BackendEngineer },
                new { QuestionId = q2, Role = Role.SoftwareEngineer },
                new { QuestionId = q3, Role = Role.FrontendEngineer },
                new { QuestionId = q3, Role = Role.FullStackEngineer },
                new { QuestionId = q4, Role = Role.SoftwareEngineer },
                new { QuestionId = q4, Role = Role.BackendEngineer }
            );

            // ===================== SEED DATA – QuestionTag =====================
            modelBuilder.Entity<QuestionTag>().HasData(
                new { QuestionId = q1, TagId = tagAlgo },
                new { QuestionId = q2, TagId = tagSysDes },
                new { QuestionId = q2, TagId = tagBackend },
                new { QuestionId = q3, TagId = tagFrontend },
                new { QuestionId = q4, TagId = tagAlgo }
            );

            // ===================== SEED DATA – Comments (legacy, kept for backward compat) =====================
            modelBuilder.Entity<Comment>().HasData(
                new Comment { Id = Guid.Parse("c1c1c1c1-1111-4c11-8c11-c1c1c1c1c111"), QuestionId = q1, Content = "Use sliding window with a HashSet. O(n) time.", IsAnswer = true, Vote = 0, CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), UpdateAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), CreateBy = user1Id, UpdateBy = user1Id },
                new Comment { Id = Guid.Parse("c2c2c2c2-2222-4c22-8c22-c2c2c2c2c222"), QuestionId = q1, Content = "You can also use a dictionary to map each character to its latest index for O(n) in a single pass.", IsAnswer = false, Vote = 0, CreatedAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc), UpdateAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc), CreateBy = user2Id, UpdateBy = user2Id },
                new Comment { Id = Guid.Parse("c3c3c3c3-3333-4c33-8c33-c3c3c3c3c333"), QuestionId = q2, Content = "Use consistent hashing, a KV store, and a redirect service.", IsAnswer = true, Vote = 0, CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), UpdateAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), CreateBy = user1Id, UpdateBy = user1Id },
                new Comment { Id = Guid.Parse("c4c4c4c4-4444-4c44-8c44-c4c4c4c4c444"), QuestionId = q2, Content = "Don't forget rate limiting and analytics counters when discussing the redirect service design.", IsAnswer = false, Vote = 0, CreatedAt = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc), UpdateAt = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc), CreateBy = user2Id, UpdateBy = user2Id },
                new Comment { Id = Guid.Parse("c5c5c5c5-5555-4c55-8c55-c5c5c5c5c555"), QuestionId = q3, Content = "== coerces types, === does not.", IsAnswer = true, Vote = 0, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), UpdateAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), CreateBy = user1Id, UpdateBy = user1Id },
                new Comment { Id = Guid.Parse("c6c6c6c6-6666-4c66-8c66-c6c6c6c6c666"), QuestionId = q3, Content = "Always prefer === in modern JS/TS to avoid implicit coercion bugs. null == undefined is true but null === undefined is false.", IsAnswer = false, Vote = 0, CreatedAt = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc), UpdateAt = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc), CreateBy = user2Id, UpdateBy = user2Id },
                new Comment { Id = Guid.Parse("c7c7c7c7-7777-4c77-8c77-c7c7c7c7c777"), QuestionId = q4, Content = "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.", IsAnswer = true, Vote = 0, CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), UpdateAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), CreateBy = user3Id, UpdateBy = user3Id },
                new Comment { Id = Guid.Parse("c8c8c8c8-8888-4c88-8c88-c8c8c8c8c888"), QuestionId = q4, Content = "Recursive solution is cleaner to write but costs O(n) stack space. Interviewers often ask you to do both.", IsAnswer = false, Vote = 0, CreatedAt = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc), UpdateAt = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc), CreateBy = user1Id, UpdateBy = user1Id }
            );

            modelBuilder.Entity<InterviewType>().HasData(
            new InterviewType
            {
                Id = Guid.Parse("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                Name = "CV Interview",
                Description = "Resume review and HR-style interview focusing on background and experience.",
                IsCoding = false,
                SuggestedDurationMinutes = 30,
                MinPrice = 1000,
                MaxPrice = 2000,
                Status = InterviewTypeStatus.Active,
                EvaluationStructureJson = """
                [
                    { "Type": "Experience Authenticity", "Question": "How well does the candidate's explanation of their past work match the details on their CV? (e.g., Did they exaggerate their contributions? Do they deeply understand the projects they listed?)" },
                    { "Type": "Communication & Presentation", "Question": "How would you rate the candidate's communication skills, clarity of expression, and overall confidence during the interview?" },
                    { "Type": "Career Alignment", "Question": "Are the candidate's short-term and long-term career goals clear, realistic, and aligned with the typical progression in this field?" },
                    { "Type": "CV Improvement (Actionable Advice)", "Question": "What is the strongest highlight of their CV? Are there any red flags, formatting issues, or vague details they need to fix immediately?" }
                ]
                """
            },
            new InterviewType
            {
                Id = Guid.Parse("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                Name = "Technical Interview",
                Description = "Technical interview with coding problems and system design questions.",
                IsCoding = true,
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 2000,
                Status = InterviewTypeStatus.Active,
                EvaluationStructureJson = """
                [
                    { "Type": "Problem Solving & Logic", "Question": "How would you evaluate the candidate's ability to analyze requirements, clarify edge cases, and approach the problem logically before writing code?" },
                    { "Type": "Code Quality & Optimization", "Question": "Rate the candidate's code quality (clean code principles, naming conventions) and their ability to optimize for time and space complexity (Big O)." },
                    { "Type": "Tech Stack & Fundamentals", "Question": "Assess the candidate's grasp of core computer science fundamentals (OOP, Databases, System Design) and their proficiency in their primary tech stack/framework." },
                    { "Type": "Actionable Tech Advice", "Question": "Where are the candidate's technical blind spots? Please list 1-3 specific technologies, concepts, or keywords they must study to improve." }
                ]
                """
            },
            new InterviewType
            {
                Id = Guid.Parse("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                Name = "Soft Skills Interview",
                Description = "Behavioral interview focused on communication and interpersonal skills.",
                IsCoding = false,
                SuggestedDurationMinutes = 60,
                MinPrice = 1000,
                MaxPrice = 2000,
                Status = InterviewTypeStatus.Active,
                EvaluationStructureJson = """
                [
                    { "Type": "Teamwork & Collaboration", "Question": "Based on the scenarios they shared, how effectively does the candidate collaborate with others, resolve conflicts, and support their teammates?" },
                    { "Type": "Adaptability & Working Under Pressure", "Question": "How does the candidate react to sudden changes in project requirements, tight deadlines, or high-pressure situations?" },
                    { "Type": "Ownership & Attitude", "Question": "Does the candidate demonstrate a strong sense of ownership (taking accountability for mistakes) and a proactive, growth-oriented mindset?" },
                    { "Type": "Professionalism Advice", "Question": "What specific advice would you give the candidate to improve their professionalism, interview etiquette, and overall impression on hiring managers?" }
                ]
                """
            },
            new InterviewType
            {
                Id = Guid.Parse("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                Name = "Mock Interview",
                Description = "Full mock interview simulating a real job interview experience.",
                IsCoding = true,
                SuggestedDurationMinutes = 90,
                MinPrice = 1000,
                MaxPrice = 2000,
                Status = InterviewTypeStatus.Active,
                EvaluationStructureJson = """
                [
                    { "Type": "Technical Readiness", "Question": "Summarize the candidate's technical competencies: Which areas meet the standard for their target level (Fresher/Junior/Mid/Senior), and which areas fall short?" },
                    { "Type": "Culture & Behavioral Fit", "Question": "Summarize their soft skills: Would this candidate be a solid cultural addition to a standard software engineering team?" },
                    { "Type": "Final Verdict", "Question": "If this were a real interview and you were the Hiring Manager, what would your decision be? (Strong Hire / Hire / Leaning Hire / No Hire) – Briefly explain your reasoning." },
                    { "Type": "Top Priorities", "Question": "List the top 3 most critical action items the candidate must execute immediately to increase their chances of passing a real job interview." }
                ]
                """
            }
        );

            modelBuilder.Entity<CoachInterviewService>().HasData(
                new CoachInterviewService
                {
                    Id = Guid.Parse("019d1466-f54f-7a12-a89e-3d459032ba89"),
                    CoachId = user2Id,
                    InterviewTypeId = Guid.Parse("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                    Price = 2000,
                    DurationMinutes = 30,
                },
                new CoachInterviewService
                {
                    Id = Guid.Parse("019d1467-d415-74d5-8d8a-de2143f27c35"),
                    CoachId = user2Id,
                    InterviewTypeId = Guid.Parse("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                    Price = 2000,
                    DurationMinutes = 60,
                },
                new CoachInterviewService
                {
                    Id = Guid.Parse("019d1467-d415-79f8-9bdc-5bb25a0b25cf"),
                    CoachId = user2Id,
                    InterviewTypeId = Guid.Parse("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                    Price = 2000,
                    DurationMinutes = 60,
                },
                new CoachInterviewService
                {
                    Id = Guid.Parse("019d1467-d415-79f8-9bdc-5bb25a0b25cd"),
                    CoachId = user2Id,
                    InterviewTypeId = Guid.Parse("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                    Price = 2000,
                    DurationMinutes = 90,
                }
            );
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }


        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is EntityDateTracking<Guid> || e.Entity is EntityDateTracking<int> || e.Entity is EntityAuditable<Guid> || e.Entity is EntityAuditable<int>);

            foreach (var entry in entries)
            {
                dynamic entity = entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
