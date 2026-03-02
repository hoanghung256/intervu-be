using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
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
        public DbSet<CoachProfile> CoachProfiles { get; set; }
        public DbSet<CoachAvailability> CoachAvailabilities { get; set; }
        public DbSet<InterviewRoom> InterviewRooms { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<InterviewBookingTransaction> InterviewBookingTransaction { get; set; }
        public DbSet<InterviewRescheduleRequest> InterviewRescheduleRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationReceive> NotificationReceives { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<InterviewType> InterviewTypes { get; set; }
        public DbSet<InterviewExperience> InterviewExperiences { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Comment> Comments { get; set; }

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
            });


            modelBuilder.Entity<CoachProfile>(b =>
            {
                b.ToTable("CoachProfiles");
                b.HasKey(x => x.Id);

                b.Property(x => x.PortfolioUrl).HasMaxLength(4000);
                b.Property(x => x.Bio).HasColumnType("text");
                b.Property(x => x.CurrentAmount);
                b.Property(x => x.BankBinNumber);
                b.Property(x => x.BankAccountNumber);
                b.Property(x => x.ExperienceYears);
                b.Property(x => x.Status).IsRequired();

                b.HasOne(x => x.User)
                 .WithOne()
                 .HasForeignKey<CoachProfile>(p => p.Id)
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
            });


            // CoachAvailability (many availabilities per coach)
            modelBuilder.Entity<CoachAvailability>(b =>
            {
                b.ToTable("CoachAvailabilities");
                b.HasKey(x => x.Id);
                b.Property(x => x.Focus)
                .HasConversion<int>()
                .IsRequired();

                b.Property(x => x.StartTime).IsRequired();
                b.Property(x => x.EndTime).IsRequired();

                b.HasOne(x => x.CoachProfile)
                .WithMany()
                .HasForeignKey(x => x.CoachId)
                .HasConstraintName("FK_CoachAvailabilities_CoachProfiles_CoachId")
                .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.ReservingForUser)
                .WithMany()
                .HasForeignKey(x => x.ReservingForUserId)
                .HasConstraintName("FK_CoachAvailabilities_Users_ReservingForUserId")
                .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<InterviewType>()
                 .WithMany()
                  .HasForeignKey(x => x.TypeId).IsRequired(false)
                  .HasConstraintName("FK_CoachAvailabilities_InterviewTypes_TypeId")
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

                // JSON converters for complex properties
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };

                b.Property(x => x.LanguageCodes)
                    .HasColumnName("LanguageCodes")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions))
                    .HasColumnType("text");

                b.Property(x => x.TestCases)
                    .HasColumnName("TestCases")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<object[]>(v, jsonOptions))
                    .HasColumnType("text");

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
                 .HasConstraintName("FK_InterviewRooms_CoachAvailabilities_CurrentAvailabilityId")
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

                b.HasOne<InterviewRoom>()
                 .WithOne()
                 .HasForeignKey<Feedback>("InterviewRoomId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<CoachProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.CoachId)
                 .HasConstraintName("FK_Feedbacks_CoachProfiles_CoachId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<CandidateProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.CandidateId)
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
                b.Property(x => x.CoachAvailabilityId).IsRequired();
                b.Property(x => x.Type).IsRequired();
                b.Property(x => x.Status).IsRequired();

                b.HasOne(x => x.CoachAvailability)
                .WithMany(x => x.InterviewBookingTransactions)
                .HasForeignKey(x => x.CoachAvailabilityId)
                .HasConstraintName("FK_InterviewBookingTransaction_CoachAvailabilities_CoachAvailabilityId")
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
                b.Property(x => x.CurrentAvailabilityId).IsRequired();
                b.Property(x => x.ProposedAvailabilityId).IsRequired();
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
                 .HasConstraintName("FK_InterviewRescheduleRequests_CoachAvailabilities_CurrentAvailabilityId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.ProposedAvailability)
                 .WithMany()
                 .HasForeignKey(x => x.ProposedAvailabilityId)
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
                b.Property(x => x.Title).HasMaxLength(255);
                b.Property(x => x.Message).HasColumnType("text");
                b.Property(x => x.CreatedAt).IsRequired();
            });

            // NotificationReceive (join)
            modelBuilder.Entity<NotificationReceive>(b =>
            {
                b.ToTable("NotificationReceives");
                b.HasKey(x => new { x.NotificationId, x.ReceiverId });

                b.HasOne<Notification>()
                 .WithMany()
                 .HasForeignKey(x => x.NotificationId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(x => x.ReceiverId)
                 .OnDelete(DeleteBehavior.Cascade);
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
                entity.ToTable("InterviewTypes");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(e => e.BasePrice)
                      .HasDefaultValue(0);

                entity.Property(e => e.Status)
                      .HasConversion<int>();
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
            var user1Id = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
            var user2Id = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
            var user3Id = Guid.Parse("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33");
            var user5Id = Guid.Parse("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44");
            var user6Id = Guid.Parse("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55");

            var room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");
            var CoachAvail1Id = Guid.Parse("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77");
            
            // Additional test data for reschedule functionality
            var CoachAvail2Id = Guid.Parse("aaaaaaaa-1111-4a1a-8a1a-111111111111"); // For reschedule testing

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

            modelBuilder.Entity<User>().HasData(user5, user6);


            modelBuilder.Entity<User>().HasData(user1, user2, user3);

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
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 2, 10, 9, 0, 0), DateTimeKind.Utc), // Match room1's ScheduledTime
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 2, 10, 10, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Booked // Already booked
                },
                // Proposed availability for reschedule (available future slot)
                new CoachAvailability
                {
                    Id = CoachAvail2Id,
                    CoachId = user2Id,
                    StartTime = DateTime.SpecifyKind(new DateTime(2026, 3, 15, 14, 0, 0), DateTimeKind.Utc), // Future date for reschedule
                    EndTime = DateTime.SpecifyKind(new DateTime(2026, 3, 15, 15, 0, 0), DateTimeKind.Utc),
                    Status = CoachAvailabilityStatus.Available
                }
            );

            // Seed transactions for testing
            var transaction1Id = Guid.Parse("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88");
            var transaction2Id = Guid.Parse("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99");

            modelBuilder.Entity<InterviewBookingTransaction>().HasData(
                new InterviewBookingTransaction
                {
                    Id = transaction1Id,
                    UserId = user1Id,
                    CoachAvailabilityId = CoachAvail1Id,
                    Amount = 1000,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Paid
                },
                new InterviewBookingTransaction
                {
                    Id = transaction2Id,
                    UserId = user2Id,
                    CoachAvailabilityId = CoachAvail1Id,
                    Amount = 500,
                    Type = TransactionType.Payout,
                    Status = TransactionStatus.Paid
                }
            );

            modelBuilder.Entity<InterviewRoom>().HasData(new InterviewRoom
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
                RescheduleAttemptCount = 0
            });

            modelBuilder.Entity<Feedback>().HasData(new Feedback
            {
                Id = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10"),
                CoachId = user2Id,
                CandidateId = user1Id,
                InterviewRoomId = room1Id,
                Rating = 5,
                Comments = "Great answers and communication.",
                AIAnalysis = "{}"
            });

            modelBuilder.Entity<Notification>().HasData(new Notification
            {
                Id = Guid.Parse("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20"),
                Title = "Welcome",
                Message = "Welcome to Intervu platform",
                CreatedAt = DateTime.SpecifyKind(new DateTime(2025, 10, 1), DateTimeKind.Utc)
            });

            modelBuilder.Entity<NotificationReceive>().HasData(new { NotificationId = Guid.Parse("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20"), ReceiverId = user1Id });

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

            // Question (interview experience sub-question)
            modelBuilder.Entity<Question>(b =>
            {
                b.ToTable("Questions");
                b.HasKey(x => x.Id);
                b.Property(x => x.QuestionType).IsRequired().HasMaxLength(200);
                b.Property(x => x.Content).IsRequired().HasColumnType("text");
                b.Property(x => x.Answer).HasColumnType("text");
                b.Property(x => x.CreatedAt).IsRequired();

                b.HasMany(x => x.Comments)
                    .WithOne(c => c.Question)
                    .HasForeignKey(c => c.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Comment (belongs to a Question; answers are stored as the first comment)
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

            // Seed InterviewExperiences
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

            modelBuilder.Entity<Question>().HasData(
                new Question
                {
                    Id = Guid.Parse("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"),
                    InterviewExperienceId = exp1Id,
                    QuestionType = "Algorithm",
                    Content = "Find the longest substring without repeating characters.",
                    Answer = "Use sliding window with a HashSet. O(n) time.",
                    CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Question
                {
                    Id = Guid.Parse("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"),
                    InterviewExperienceId = exp1Id,
                    QuestionType = "System Design",
                    Content = "Design a URL shortener like bit.ly.",
                    Answer = "Use consistent hashing, a KV store, and a redirect service.",
                    CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Question
                {
                    Id = Guid.Parse("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"),
                    InterviewExperienceId = exp2Id,
                    QuestionType = "JavaScript",
                    Content = "Explain the difference between == and ===.",
                    Answer = "== coerces types, === does not.",
                    CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new Question
                {
                    Id = Guid.Parse("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"),
                    InterviewExperienceId = exp3Id,
                    QuestionType = "Algorithm",
                    Content = "Reverse a linked list.",
                    Answer = "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.",
                    CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Comments – answers are stored as the first comment (IsAnswer = true) plus additional community comments
            modelBuilder.Entity<Comment>().HasData(
                // Q1: longest substring – answer comment
                new Comment
                {
                    Id = Guid.Parse("c1c1c1c1-1111-4c11-8c11-c1c1c1c1c111"),
                    QuestionId = Guid.Parse("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"),
                    Content = "Use sliding window with a HashSet. O(n) time.",
                    IsAnswer = true,
                    Vote = 10,
                    CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    UpdateAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    CreateBy = user1Id,
                    UpdateBy = user1Id
                },
                // Q1: extra community comment
                new Comment
                {
                    Id = Guid.Parse("c2c2c2c2-2222-4c22-8c22-c2c2c2c2c222"),
                    QuestionId = Guid.Parse("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"),
                    Content = "You can also use a dictionary to map each character to its latest index for O(n) in a single pass.",
                    IsAnswer = false,
                    Vote = 5,
                    CreatedAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                    UpdateAt = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                    CreateBy = user2Id,
                    UpdateBy = user2Id
                },
                // Q2: URL shortener – answer comment
                new Comment
                {
                    Id = Guid.Parse("c3c3c3c3-3333-4c33-8c33-c3c3c3c3c333"),
                    QuestionId = Guid.Parse("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"),
                    Content = "Use consistent hashing, a KV store, and a redirect service.",
                    IsAnswer = true,
                    Vote = 8,
                    CreatedAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    UpdateAt = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    CreateBy = user1Id,
                    UpdateBy = user1Id
                },
                // Q2: extra community comment
                new Comment
                {
                    Id = Guid.Parse("c4c4c4c4-4444-4c44-8c44-c4c4c4c4c444"),
                    QuestionId = Guid.Parse("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"),
                    Content = "Don't forget rate limiting and analytics counters when discussing the redirect service design.",
                    IsAnswer = false,
                    Vote = 3,
                    CreatedAt = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc),
                    UpdateAt = new DateTime(2026, 1, 13, 0, 0, 0, DateTimeKind.Utc),
                    CreateBy = user2Id,
                    UpdateBy = user2Id
                },
                // Q3: == vs === – answer comment
                new Comment
                {
                    Id = Guid.Parse("c5c5c5c5-5555-4c55-8c55-c5c5c5c5c555"),
                    QuestionId = Guid.Parse("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"),
                    Content = "== coerces types, === does not.",
                    IsAnswer = true,
                    Vote = 7,
                    CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    UpdateAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    CreateBy = user1Id,
                    UpdateBy = user1Id
                },
                // Q3: extra community comment
                new Comment
                {
                    Id = Guid.Parse("c6c6c6c6-6666-4c66-8c66-c6c6c6c6c666"),
                    QuestionId = Guid.Parse("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"),
                    Content = "Always prefer === in modern JS/TS to avoid implicit coercion bugs. null == undefined is true but null === undefined is false.",
                    IsAnswer = false,
                    Vote = 4,
                    CreatedAt = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc),
                    UpdateAt = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc),
                    CreateBy = user2Id,
                    UpdateBy = user2Id
                },
                // Q4: reverse linked list – answer comment
                new Comment
                {
                    Id = Guid.Parse("c7c7c7c7-7777-4c77-8c77-c7c7c7c7c777"),
                    QuestionId = Guid.Parse("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"),
                    Content = "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.",
                    IsAnswer = true,
                    Vote = 9,
                    CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdateAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreateBy = user3Id,
                    UpdateBy = user3Id
                },
                // Q4: extra community comment
                new Comment
                {
                    Id = Guid.Parse("c8c8c8c8-8888-4c88-8c88-c8c8c8c8c888"),
                    QuestionId = Guid.Parse("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"),
                    Content = "Recursive solution is cleaner to write but costs O(n) stack space. Interviewers often ask you to do both.",
                    IsAnswer = false,
                    Vote = 6,
                    CreatedAt = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                    UpdateAt = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                    CreateBy = user1Id,
                    UpdateBy = user1Id
                }
            );

            modelBuilder.Entity<InterviewType>().HasData(
                new InterviewType
                {
                    Id = Guid.Parse("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                    Name = "CV Interview",
                    Description = "Resume review and HR-style interview focusing on background and experience.",
                    IsCoding = false,
                    DurationMinutes = 30,
                    BasePrice = 20,
                    Status = InterviewTypeStatus.Active
                },
                new InterviewType
                {
                    Id = Guid.Parse("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"),
                    Name = "Technical Interview",
                    Description = "Technical interview with coding problems and system design questions.",
                    IsCoding = true,
                    DurationMinutes = 60,
                    BasePrice = 50,
                    Status = InterviewTypeStatus.Active
                },
                new InterviewType
                {
                    Id = Guid.Parse("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                    Name = "Soft Skills Interview",
                    Description = "Behavioral interview focused on communication and interpersonal skills.",
                    IsCoding = false,
                    DurationMinutes = 45,
                    BasePrice = 30,
                    Status = InterviewTypeStatus.Active
                },
                new InterviewType
                {
                    Id = Guid.Parse("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                    Name = "Mock Interview",
                    Description = "Full mock interview simulating a real job interview experience.",
                    IsCoding = true,
                    DurationMinutes = 75,
                    BasePrice = 70,
                    Status = InterviewTypeStatus.Draft
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
