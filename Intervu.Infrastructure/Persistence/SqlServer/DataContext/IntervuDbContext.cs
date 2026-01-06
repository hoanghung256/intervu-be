using Intervu.Domain.Abstractions.Entity;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;

namespace Intervu.Infrastructure.Persistence.SqlServer.DataContext
{
    public class IntervuDbContext : DbContext
    {
        public IntervuDbContext()
        {
        }

        public IntervuDbContext(DbContextOptions<IntervuDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<IntervieweeProfile> IntervieweeProfiles { get; set; }
        public DbSet<InterviewerProfile> InterviewerProfiles { get; set; }
        public DbSet<InterviewerAvailability> InterviewerAvailabilities { get; set; }
        public DbSet<InterviewRoom> InterviewRooms { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<InterviewBookingTransaction> InterviewBookingTransaction { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationReceive> NotificationReceives { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Skill> Skills { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost;Database=Intervu;Trusted_Connection=True;TrustServerCertificate=True;");
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
            });

            // IntervieweeProfile (one-to-one with User, shared PK)
            modelBuilder.Entity<IntervieweeProfile>(b =>
            {
                b.ToTable("IntervieweeProfiles");
                b.HasKey(x => x.Id);
                b.Property(x => x.CVUrl).HasMaxLength(1000);
                b.Property(x => x.PortfolioUrl).HasMaxLength(1000);
                b.Property(x => x.Bio).HasColumnType("nvarchar(max)");

                // Explicitly map navigation to User (like InterviewerProfile)
                b.HasOne(x => x.User)
                 .WithOne()
                 .HasForeignKey<IntervieweeProfile>(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);

                // Many-to-many: IntervieweeProfile <-> Skill
                b.HasMany(x => x.Skills)
                 .WithMany()
                 .UsingEntity<Dictionary<string, object>>(
                     "IntervieweeSkills",
                     l => l.HasOne<Skill>().WithMany().HasForeignKey("SkillsId").OnDelete(DeleteBehavior.Cascade),
                     r => r.HasOne<IntervieweeProfile>().WithMany().HasForeignKey("IntervieweeProfilesId").OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey("IntervieweeProfilesId", "SkillsId");
                         j.ToTable("IntervieweeSkills");
                     });
            });


            modelBuilder.Entity<InterviewerProfile>(b =>
            {
                b.ToTable("InterviewerProfiles");
                b.HasKey(x => x.Id);

                b.Property(x => x.PortfolioUrl).HasMaxLength(4000);
                b.Property(x => x.Bio).HasColumnType("nvarchar(max)");
                b.Property(x => x.CurrentAmount);
                b.Property(x => x.BankBinNumber);
                b.Property(x => x.BankAccountNumber);
                b.Property(x => x.ExperienceYears);
                b.Property(x => x.Status).IsRequired();

                b.HasOne(x => x.User)
                 .WithOne()
                 .HasForeignKey<InterviewerProfile>(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.Companies)
                 .WithMany(c => c.InterviewerProfiles)
                 .UsingEntity<Dictionary<string, object>>(
                     "InterviewerCompanies",
                     l => l.HasOne<Company>().WithMany().HasForeignKey("CompaniesId").OnDelete(DeleteBehavior.Cascade),
                     r => r.HasOne<InterviewerProfile>().WithMany().HasForeignKey("InterviewerProfilesId").OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey("InterviewerProfilesId", "CompaniesId");
                         j.ToTable("InterviewerCompanies");
                     });

                b.HasMany(x => x.Skills)
                 .WithMany(s => s.InterviewerProfiles)
                 .UsingEntity<Dictionary<string, object>>(
                     "InterviewerSkills",
                     l => l.HasOne<Skill>().WithMany().HasForeignKey("SkillsId").OnDelete(DeleteBehavior.Cascade),
                     r => r.HasOne<InterviewerProfile>().WithMany().HasForeignKey("InterviewerProfilesId").OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey("InterviewerProfilesId", "SkillsId");
                         j.ToTable("InterviewerSkills");
                     });
            });


            // InterviewerAvailability (many availabilities per interviewer)
            modelBuilder.Entity<InterviewerAvailability>(b =>
            {
                b.ToTable("InterviewerAvailabilities");
                b.HasKey(x => x.Id);
                b.Property(x => x.StartTime).IsRequired();
                b.Property(x => x.EndTime).IsRequired();

                b.HasOne<InterviewerProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.InterviewerId)
                 .OnDelete(DeleteBehavior.Cascade);
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
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions))
                    .HasColumnType("nvarchar(max)");

                b.Property(x => x.TestCases)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<object[]>(v, jsonOptions))
                    .HasColumnType("nvarchar(max)");

                b.Property(x => x.CurrentLanguage).HasMaxLength(50);
                b.Property(x => x.ProblemDescription).HasColumnType("nvarchar(max)");
                b.Property(x => x.ProblemShortName).HasMaxLength(200);

                b.HasOne<IntervieweeProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<InterviewerProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.InterviewerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Feedback
            modelBuilder.Entity<Feedback>(b =>
            {
                b.ToTable("Feedbacks");
                b.HasKey(x => x.Id);
                b.Property(x => x.Rating).IsRequired();
                b.Property(x => x.Comments).HasColumnType("nvarchar(max)");
                b.Property(x => x.AIAnalysis).HasColumnType("nvarchar(max)");

                b.Property<int>("InterviewRoomId").IsRequired();

                b.HasOne<InterviewRoom>()
                 .WithOne()
                 .HasForeignKey<Feedback>("InterviewRoomId")
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<InterviewerProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.InterviewerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<IntervieweeProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Transaction
            modelBuilder.Entity<InterviewBookingTransaction>(b =>
            {
                b.ToTable("InterviewBookingTransaction");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).IsRequired();
                //b.Property(x => x.CreatedAt).IsRequired();
                //b.Property(x => x.UpdatedAt).IsRequired();
                b.Property(x => x.InterviewerAvailabilityId).IsRequired();
                b.Property(x => x.Type).IsRequired();
                b.Property(x => x.Status).IsRequired();

                b.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification
            modelBuilder.Entity<Notification>(b =>
            {
                b.ToTable("Notifications");
                b.HasKey(x => x.Id);
                b.Property(x => x.Title).HasMaxLength(255);
                b.Property(x => x.Message).HasColumnType("nvarchar(max)");
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
                b.Property(x => x.Description).HasColumnType("nvarchar(max)");
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

            // Seed data (Guid-based, consistent with PostgreSQL)
            var user1Id = Guid.Parse("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11");
            var user2Id = Guid.Parse("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22");
            var user3Id = Guid.Parse("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33");
            var user5Id = Guid.Parse("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44");
            var user6Id = Guid.Parse("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55");

            var room1Id = Guid.Parse("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66");
            var interviewerAvail1Id = Guid.Parse("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77");

            var user1 = new User
            {
                Id = user1Id,
                FullName = "Alice Student",
                Email = "alice@example.com",
                Password = "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=",
                Role = UserRole.Interviewee,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            var user2 = new User
            {
                Id = user2Id,
                FullName = "Bob Interviewer",
                Email = "bob@example.com",
                Password = "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=",
                Role = UserRole.Interviewer,
                ProfilePicture = null,
                Status = UserStatus.Active,
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
            };

            var user5 = new User
            {
                Id = user5Id,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Password = user1.Password,
                Role = UserRole.Interviewer,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            var user6 = new User
            {
                Id = user6Id,
                FullName = "Sarah Lee",
                Email = "sarah.lee@example.com",
                Password = user1.Password,
                Role = UserRole.Interviewer,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            modelBuilder.Entity<User>().HasData(user5, user6);
            modelBuilder.Entity<User>().HasData(user1, user2, user3);

            modelBuilder.Entity<IntervieweeProfile>().HasData(new IntervieweeProfile
            {
                Id = user1Id,
                CVUrl = "https://example.com/cv-alice.pdf",
                PortfolioUrl = "https://portfolio.example.com/alice",
                Bio = "Aspiring backend developer."
            });

            modelBuilder.Entity("IntervieweeSkills").HasData(
                new { IntervieweeProfilesId = user1Id, SkillsId = Guid.Parse("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") },
                new { IntervieweeProfilesId = user1Id, SkillsId = Guid.Parse("02020202-0202-4202-8202-020202020202") }
            );

            modelBuilder.Entity<InterviewerProfile>().HasData(
            new InterviewerProfile
            {
                Id = user2Id,
                PortfolioUrl = "https://portfolio.example.com/bob",
                ExperienceYears = 8,
                Status = InterviewerProfileStatus.Enable,
                CurrentAmount = 0,
                Bio = "Senior Backend Engineer with real interview experience"
            },
            new InterviewerProfile
            {
                Id = user5Id,
                PortfolioUrl = "https://portfolio.example.com/john",
                ExperienceYears = 6,
                CurrentAmount = 0,
                Bio = "Fullstack Engineer previously at Uber",
                Status = InterviewerProfileStatus.Enable
            },
            new InterviewerProfile
            {
                Id = user6Id,
                PortfolioUrl = "https://portfolio.example.com/sarah",
                ExperienceYears = 7,
                CurrentAmount = 0,
                Bio = "Senior Frontend Engineer focusing on UI/UX interviews",
                Status = InterviewerProfileStatus.Enable
            }
            );

            modelBuilder.Entity<InterviewerAvailability>().HasData(new InterviewerAvailability
            {
                Id = interviewerAvail1Id,
                InterviewerId = user2Id,
                StartTime = DateTime.SpecifyKind(new DateTime(2025, 11, 1, 9, 0, 0), DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(new DateTime(2025, 11, 1, 10, 0, 0), DateTimeKind.Utc),
                IsBooked = false
            });

            modelBuilder.Entity<InterviewRoom>().HasData(new InterviewRoom
            {
                Id = room1Id,
                StudentId = user1Id,
                InterviewerId = user2Id,
                ScheduledTime = DateTime.SpecifyKind(new DateTime(2025, 11, 1, 9, 0, 0), DateTimeKind.Utc),
                DurationMinutes = 60,
                VideoCallRoomUrl = "https://meet.example/room1",
                Status = InterviewRoomStatus.Scheduled
            });

            modelBuilder.Entity<InterviewBookingTransaction>().HasData(new InterviewBookingTransaction
            {
                Id = Guid.Parse("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"),
                UserId = user1Id,
                InterviewerAvailabilityId = interviewerAvail1Id,
                Amount = 1000,
                Type = TransactionType.Payment,
                Status = TransactionStatus.Paid,
            },
            new InterviewBookingTransaction
            {
                Id = Guid.Parse("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"),
                UserId = user2Id,
                InterviewerAvailabilityId = interviewerAvail1Id,
                Amount = 500,
                Type = TransactionType.Payout,
                Status = TransactionStatus.Paid,
            });

            modelBuilder.Entity<Feedback>().HasData(new Feedback
            {
                Id = Guid.Parse("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10"),
                InterviewerId = user2Id,
                StudentId = user1Id,
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
                new Company { Id = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"), Name = "Stripe", Website = "https://stripe.com", LogoPath = "logos/stripe.png" }
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
            modelBuilder.Entity("InterviewerCompanies").HasData(
                new { InterviewerProfilesId = user2Id, CompaniesId = Guid.Parse("11111111-1111-4111-8111-111111111111") }, // Google
                new { InterviewerProfilesId = user2Id, CompaniesId = Guid.Parse("44444444-4444-4444-8444-444444444444") }, // Microsoft
                new { InterviewerProfilesId = user2Id, CompaniesId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa") } // Stripe
            );

            // John (user5Id)
            modelBuilder.Entity("InterviewerCompanies").HasData(
                new { InterviewerProfilesId = user5Id, CompaniesId = Guid.Parse("88888888-8888-4888-8888-888888888888") }, // Uber
                new { InterviewerProfilesId = user5Id, CompaniesId = Guid.Parse("33333333-3333-4333-8333-333333333333") }, // Amazon
                new { InterviewerProfilesId = user5Id, CompaniesId = Guid.Parse("66666666-6666-4666-8666-666666666666") }  // TikTok
            );

            // Sarah (user6Id)
            modelBuilder.Entity("InterviewerCompanies").HasData(
                new { InterviewerProfilesId = user6Id, CompaniesId = Guid.Parse("77777777-7777-4777-8777-777777777777") }, // Apple
                new { InterviewerProfilesId = user6Id, CompaniesId = Guid.Parse("99999999-9999-4999-8999-999999999999") }, // Spotify
                new { InterviewerProfilesId = user6Id, CompaniesId = Guid.Parse("22222222-2222-4222-8222-222222222222") }  // Meta
            );

            // Bob (backend)
            modelBuilder.Entity("InterviewerSkills").HasData(
                new { InterviewerProfilesId = user2Id, SkillsId = Guid.Parse("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") },
                new { InterviewerProfilesId = user2Id, SkillsId = Guid.Parse("02020202-0202-4202-8202-020202020202") },
                new { InterviewerProfilesId = user2Id, SkillsId = Guid.Parse("06060606-0606-4606-8606-060606060606") },
                new { InterviewerProfilesId = user2Id, SkillsId = Guid.Parse("07070707-0707-4707-8707-070707070707") },
                new { InterviewerProfilesId = user2Id, SkillsId = Guid.Parse("08080808-0808-4808-8808-080808080808") }
            );

            // John (fullstack)
            modelBuilder.Entity("InterviewerSkills").HasData(
                new { InterviewerProfilesId = 5, SkillsId = 3 },
                new { InterviewerProfilesId = 5, SkillsId = 4 },
                new { InterviewerProfilesId = 5, SkillsId = 12 },
                new { InterviewerProfilesId = 5, SkillsId = 9 },
                new { InterviewerProfilesId = 5, SkillsId = 14 }
            );

            // Sarah (frontend + ML)
            modelBuilder.Entity("InterviewerSkills").HasData(
                new { InterviewerProfilesId = 6, SkillsId = 3 },
                new { InterviewerProfilesId = 6, SkillsId = 4 },
                new { InterviewerProfilesId = 6, SkillsId = 5 },
                new { InterviewerProfilesId = 6, SkillsId = 15 }
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
