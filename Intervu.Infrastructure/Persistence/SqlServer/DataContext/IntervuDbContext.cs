using Intervu.Domain.Abstractions.Entities;
using Intervu.Domain.Abstractions.Entities.Interfaces;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq.Expressions;
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
        public DbSet<Payment> Payments { get; set; }
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
                b.Property(x => x.Skills).HasColumnType("nvarchar(max)");
                b.Property(x => x.Bio).HasColumnType("nvarchar(max)");

                b.HasOne<User>()
                 .WithOne()
                 .HasForeignKey<IntervieweeProfile>(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<InterviewerProfile>(b =>
            {
                b.ToTable("InterviewerProfiles");
                b.HasKey(x => x.Id);

                b.Property(x => x.PortfolioUrl).HasMaxLength(4000);
                b.Property(x => x.Bio).HasColumnType("nvarchar(max)");
                b.Property(x => x.CurrentAmount);
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

                b.HasOne<InterviewerProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.InterviewerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<IntervieweeProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.StudentId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment
            modelBuilder.Entity<Payment>(b =>
            {
                b.ToTable("Payments");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
                b.Property(x => x.PaymentMethod).HasMaxLength(200);
                b.Property(x => x.TransactionDate).IsRequired();
                b.Property(x => x.Status).IsRequired();

                b.HasOne<InterviewRoom>()
                 .WithMany()
                 .HasForeignKey(x => x.InterviewRoomId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne<IntervieweeProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.IntervieweeId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne<InterviewerProfile>()
                 .WithMany()
                 .HasForeignKey(x => x.InterviewerId)
                 .OnDelete(DeleteBehavior.Restrict);
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

            // Seed data
            var user1 = new User
            {
                Id = 1,
                FullName = "Alice Student",
                Email = "alice@example.com",
                Password = "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=",
                Role = UserRole.Interviewee,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            var user2 = new User
            {
                Id = 2,
                FullName = "Bob Interviewer",
                Email = "bob@example.com",
                Password = "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=",
                Role = UserRole.Interviewer,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            var user3 = new User
            {
                Id = 3,
                FullName = "Admin",
                Email = "admin@example.com",
                Password = "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=",
                Role = UserRole.Admin,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            var user5 = new User
            {
                Id = 5,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Password = user1.Password,
                Role = UserRole.Interviewer,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            var user6 = new User
            {
                Id = 6,
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
                Id = 1,
                CVUrl = "https://example.com/cv-alice.pdf",
                PortfolioUrl = "https://portfolio.example.com/alice",
                Skills = "[C#, SQL]",
                Bio = "Aspiring backend developer."
            });

            modelBuilder.Entity<InterviewerProfile>().HasData(
            new InterviewerProfile
            {
                Id = 2,
                PortfolioUrl = "https://portfolio.example.com/bob",
                ExperienceYears = 8,
                Status = InterviewerProfileStatus.Enable,
                CurrentAmount = 0,
                Bio = "Senior Backend Engineer with real interview experience"
            },
            new InterviewerProfile
            {
                Id = 5,
                PortfolioUrl = "https://portfolio.example.com/john",
                ExperienceYears = 6,
                CurrentAmount = 0,
                Bio = "Fullstack Engineer previously at Uber",
                Status = InterviewerProfileStatus.Enable
            },
            new InterviewerProfile
            {
                Id = 6,
                PortfolioUrl = "https://portfolio.example.com/sarah",
                ExperienceYears = 7,
                CurrentAmount = 0,
                Bio = "Senior Frontend Engineer focusing on UI/UX interviews",
                Status = InterviewerProfileStatus.Enable
            }
            );

            modelBuilder.Entity<InterviewerAvailability>().HasData(new InterviewerAvailability
            {
                Id = 1,
                InterviewerId = 2,
                StartTime = new DateTime(2025, 11, 1, 9, 0, 0),
                EndTime = new DateTime(2025, 11, 1, 10, 0, 0),
                IsBooked = false
            });

            modelBuilder.Entity<InterviewRoom>().HasData(new InterviewRoom
            {
                Id = 1,
                StudentId = 1,
                InterviewerId = 2,
                ScheduledTime = new DateTime(2025, 11, 1, 9, 0, 0),
                DurationMinutes = 60,
                VideoCallRoomUrl = "https://meet.example/room1",
                Status = InterviewRoomStatus.Scheduled
            });

            modelBuilder.Entity<Payment>().HasData(new Payment
            {
                Id = 1,
                InterviewRoomId = 1,
                IntervieweeId = 1,
                InterviewerId = 2,
                Amount = 50.00m,
                PaymentMethod = "Card",
                TransactionDate = new DateTime(2025, 10, 1),
                Status = PaymentStatus.Pending
            });

            modelBuilder.Entity<Feedback>().HasData(new Feedback
            {
                Id = 1,
                InterviewerId = 2,
                StudentId = 1,
                Rating = 5,
                Comments = "Great answers and communication.",
                AIAnalysis = "{}"
            });

            modelBuilder.Entity<Notification>().HasData(new Notification
            {
                Id = 1,
                Title = "Welcome",
                Message = "Welcome to Intervu platform",
                CreatedAt = new DateTime(2025, 10, 1)
            });

            modelBuilder.Entity<NotificationReceive>().HasData(new { NotificationId = 1, ReceiverId = 1 });

            modelBuilder.Entity<Company>().HasData(
                new Company { Id = 1, Name = "Google", Website = "https://google.com", LogoPath = "logos/google.png" },
                new Company { Id = 2, Name = "Meta", Website = "https://meta.com", LogoPath = "logos/meta.png" },
                new Company { Id = 3, Name = "Amazon", Website = "https://amazon.com", LogoPath = "logos/amazon.png" },
                new Company { Id = 4, Name = "Microsoft", Website = "https://microsoft.com", LogoPath = "logos/microsoft.png" },
                new Company { Id = 5, Name = "Netflix", Website = "https://netflix.com", LogoPath = "logos/netflix.png" },
                new Company { Id = 6, Name = "TikTok", Website = "https://tiktok.com", LogoPath = "logos/tiktok.png" },
                new Company { Id = 7, Name = "Apple", Website = "https://apple.com", LogoPath = "logos/apple.png" },
                new Company { Id = 8, Name = "Uber", Website = "https://uber.com", LogoPath = "logos/uber.png" },
                new Company { Id = 9, Name = "Spotify", Website = "https://spotify.com", LogoPath = "logos/spotify.png" },
                new Company { Id = 10, Name = "Stripe", Website = "https://stripe.com", LogoPath = "logos/stripe.png" }
            );

            modelBuilder.Entity<Skill>().HasData(
                new Skill { Id = 1, Name = "C#" },
                new Skill { Id = 2, Name = "Java" },
                new Skill { Id = 3, Name = "JavaScript" },
                new Skill { Id = 4, Name = "TypeScript" },
                new Skill { Id = 5, Name = "React" },
                new Skill { Id = 6, Name = "Node.js" },
                new Skill { Id = 7, Name = "SQL" },
                new Skill { Id = 8, Name = "MongoDB" },
                new Skill { Id = 9, Name = "AWS" },
                new Skill { Id = 10, Name = "Azure" },
                new Skill { Id = 11, Name = "System Design" },
                new Skill { Id = 12, Name = "Microservices" },
                new Skill { Id = 13, Name = "Docker" },
                new Skill { Id = 14, Name = "Kubernetes" },
                new Skill { Id = 15, Name = "Machine Learning" }
            );

            // Bob (2)
            modelBuilder.Entity("InterviewerCompanies").HasData(
                new { InterviewerProfilesId = 2, CompaniesId = 1 }, // Google
                new { InterviewerProfilesId = 2, CompaniesId = 4 }, // Microsoft
                new { InterviewerProfilesId = 2, CompaniesId = 10 } // Stripe
            );

            // John (5)
            modelBuilder.Entity("InterviewerCompanies").HasData(
                new { InterviewerProfilesId = 5, CompaniesId = 8 }, // Uber
                new { InterviewerProfilesId = 5, CompaniesId = 3 }, // Amazon
                new { InterviewerProfilesId = 5, CompaniesId = 6 }  // TikTok
            );

            // Sarah (6)
            modelBuilder.Entity("InterviewerCompanies").HasData(
                new { InterviewerProfilesId = 6, CompaniesId = 7 }, // Apple
                new { InterviewerProfilesId = 6, CompaniesId = 9 }, // Spotify
                new { InterviewerProfilesId = 6, CompaniesId = 2 }  // Meta
            );

            // Bob (backend)
            modelBuilder.Entity("InterviewerSkills").HasData(
                new { InterviewerProfilesId = 2, SkillsId = 1 },
                new { InterviewerProfilesId = 2, SkillsId = 7 },
                new { InterviewerProfilesId = 2, SkillsId = 11 },
                new { InterviewerProfilesId = 2, SkillsId = 12 },
                new { InterviewerProfilesId = 2, SkillsId = 13 }
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
                .Where(e => e.Entity is EntityAudit<Guid> || e.Entity is EntityAudit<int> || e.Entity is EntityAuditSoftDelete<Guid> || e.Entity is EntityAuditSoftDelete<int>);

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
