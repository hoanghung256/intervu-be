using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

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

            // InterviewerProfile (one-to-one with User, shared PK)
            modelBuilder.Entity<InterviewerProfile>(b =>
            {
                b.ToTable("InterviewerProfiles");
                b.HasKey(x => x.Id);
                b.Property(x => x.CVUrl).HasMaxLength(4000);
                b.Property(x => x.PortfolioUrl).HasMaxLength(4000);
                b.Property(x => x.Specializations).HasColumnType("nvarchar(max)");
                b.Property(x => x.ProgrammingLanguages).HasColumnType("nvarchar(max)");
                b.Property(x => x.Company).HasMaxLength(200);
                b.Property(x => x.Bio).HasColumnType("nvarchar(max)");

                b.HasOne<User>()
                 .WithOne()
                 .HasForeignKey<InterviewerProfile>(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);
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
                b.Property(x => x.ScheduledTime).IsRequired();
                b.Property(x => x.DurationMinutes).IsRequired();
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

            // Seed data
            var user1 = new User
            {
                Id = 1,
                FullName = "Alice Student",
                Email = "alice@example.com",
                Password = "hashedpassword",
                Role = UserRole.Interviewee,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            var user2 = new User
            {
                Id = 2,
                FullName = "Bob Interviewer",
                Email = "bob@example.com",
                Password = "hashedpassword",
                Role = UserRole.Interviewer,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            var user3 = new User
            {
                Id = 3,
                FullName = "Admin",
                Email = "admin@example.com",
                Password = "hashedpassword",
                Role = UserRole.Admin,
                ProfilePicture = null,
                Status = UserStatus.Active,
            };

            modelBuilder.Entity<User>().HasData(user1, user2, user3);

            modelBuilder.Entity<IntervieweeProfile>().HasData(new IntervieweeProfile
            {
                Id = 1,
                CVUrl = "https://example.com/cv-alice.pdf",
                PortfolioUrl = "https://portfolio.example.com/alice",
                Skills = "[C#, SQL]",
                Bio = "Aspiring backend developer."
            });

            modelBuilder.Entity<InterviewerProfile>().HasData(new InterviewerProfile
            {
                Id = 2,
                CVUrl = "https://example.com/cv-bob.pdf",
                PortfolioUrl = "https://portfolio.example.com/bob",
                Specializations = "Backend, System Design",
                ProgrammingLanguages = "C#, JavaScript",
                Company = "Tech Co",
                ExperienceYears = 8,
                Bio = "Senior software engineer",
                IsVerified = true
            });

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
        }
    }
}
