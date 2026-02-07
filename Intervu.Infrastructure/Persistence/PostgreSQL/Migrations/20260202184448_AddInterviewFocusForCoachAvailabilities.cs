using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewFocusForCoachAvailabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LogoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsCoding = table.Column<bool>(type: "boolean", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    BasePrice = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    ProfilePicture = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SlugProfileUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CandidateProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CVUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PortfolioUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    CurrentAmount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateProfiles_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioUrl = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CurrentAmount = table.Column<int>(type: "integer", nullable: true),
                    ExperienceYears = table.Column<int>(type: "integer", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: false),
                    BankBinNumber = table.Column<string>(type: "text", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachProfiles_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationReceives",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationReceives", x => new { x.NotificationId, x.ReceiverId });
                    table.ForeignKey(
                        name: "FK_NotificationReceives_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationReceives_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidateSkills",
                columns: table => new
                {
                    CandidateProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSkills", x => new { x.CandidateProfilesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_CandidateSkills_CandidateProfiles_CandidateProfilesId",
                        column: x => x.CandidateProfilesId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CandidateSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Focus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachAvailabilities_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachAvailabilities_InterviewTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "InterviewTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CoachCompanies",
                columns: table => new
                {
                    CoachProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompaniesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachCompanies", x => new { x.CoachProfilesId, x.CompaniesId });
                    table.ForeignKey(
                        name: "FK_CoachCompanies_CoachProfiles_CoachProfilesId",
                        column: x => x.CoachProfilesId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachCompanies_Companies_CompaniesId",
                        column: x => x.CompaniesId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachSkills",
                columns: table => new
                {
                    CoachProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachSkills", x => new { x.CoachProfilesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_CoachSkills_CoachProfiles_CoachProfilesId",
                        column: x => x.CoachProfilesId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoachSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    VideoCallRoomUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CurrentLanguage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LanguageCodes = table.Column<string>(type: "text", nullable: true),
                    ProblemDescription = table.Column<string>(type: "text", nullable: true),
                    ProblemShortName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TestCases = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_CandidateProfiles_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InterviewBookingTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachAvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewBookingTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewBookingTransaction_CoachAvailabilities_CoachAvailabilityId",
                        column: x => x.CoachAvailabilityId,
                        principalTable: "CoachAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewBookingTransaction_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: false),
                    AIAnalysis = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_CandidateProfiles_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_CoachProfiles_CoachId",
                        column: x => x.CoachId,
                        principalTable: "CoachProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_InterviewRooms_InterviewRoomId",
                        column: x => x.InterviewRoomId,
                        principalTable: "InterviewRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "LogoPath", "Name", "Website" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), "logos/google.png", "Google", "https://google.com" },
                    { new Guid("22222222-2222-4222-8222-222222222222"), "logos/meta.png", "Meta", "https://meta.com" },
                    { new Guid("33333333-3333-4333-8333-333333333333"), "logos/amazon.png", "Amazon", "https://amazon.com" },
                    { new Guid("44444444-4444-4444-8444-444444444444"), "logos/microsoft.png", "Microsoft", "https://microsoft.com" },
                    { new Guid("55555555-5555-4555-8555-555555555555"), "logos/netflix.png", "Netflix", "https://netflix.com" },
                    { new Guid("66666666-6666-4666-8666-666666666666"), "logos/tiktok.png", "TikTok", "https://tiktok.com" },
                    { new Guid("77777777-7777-4777-8777-777777777777"), "logos/apple.png", "Apple", "https://apple.com" },
                    { new Guid("88888888-8888-4888-8888-888888888888"), "logos/uber.png", "Uber", "https://uber.com" },
                    { new Guid("99999999-9999-4999-8999-999999999999"), "logos/spotify.png", "Spotify", "https://spotify.com" },
                    { new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"), "logos/stripe.png", "Stripe", "https://stripe.com" }
                });

            migrationBuilder.InsertData(
                table: "InterviewTypes",
                columns: new[] { "Id", "BasePrice", "Description", "DurationMinutes", "IsCoding", "Name", "Status" },
                values: new object[,]
                {
                    { new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"), 30, "Behavioral interview focused on communication and interpersonal skills.", 45, false, "Soft Skills Interview", 1 },
                    { new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"), 20, "Resume review and HR-style interview focusing on background and experience.", 30, false, "CV Interview", 1 },
                    { new Guid("e8b74d9f-2c41-4c9a-9b13-1f8a6e52d0c3"), 50, "Technical interview with coding problems and system design questions.", 60, true, "Technical Interview", 1 },
                    { new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"), 70, "Full mock interview simulating a real job interview experience.", 75, true, "Mock Interview", 0 }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "CreatedAt", "Message", "Title" },
                values: new object[] { new Guid("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20"), new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Welcome to Intervu platform", "Welcome" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("01010101-0101-4101-8101-010101010101"), null, "Node.js" },
                    { new Guid("02020202-0202-4202-8202-020202020202"), null, "SQL" },
                    { new Guid("03030303-0303-4303-8303-030303030303"), null, "MongoDB" },
                    { new Guid("04040404-0404-4404-8404-040404040404"), null, "AWS" },
                    { new Guid("05050505-0505-4505-8505-050505050505"), null, "Azure" },
                    { new Guid("06060606-0606-4606-8606-060606060606"), null, "System Design" },
                    { new Guid("07070707-0707-4707-8707-070707070707"), null, "Microservices" },
                    { new Guid("08080808-0808-4808-8808-080808080808"), null, "Docker" },
                    { new Guid("09090909-0909-4909-8909-090909090909"), null, "Kubernetes" },
                    { new Guid("0a0a0a0a-0a0a-4a0a-8a0a-0a0a0a0a0a0a"), null, "Machine Learning" },
                    { new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1"), null, "C#" },
                    { new Guid("c2c2c2c2-c2c2-42c2-82c2-c2c2c2c2c2c2"), null, "Java" },
                    { new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3"), null, "JavaScript" },
                    { new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4"), null, "TypeScript" },
                    { new Guid("f5f5f5f5-f5f5-45f5-85f5-f5f5f5f5f5f5"), null, "React" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Password", "ProfilePicture", "Role", "SlugProfileUrl", "Status" },
                values: new object[,]
                {
                    { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "alice@example.com", "Alice Candidate", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 0, "alice-candidate_1719000000001", 0 },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "bob@example.com", "Bob Coach", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, "bob-Coach_1719000000002", 0 },
                    { new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), "admin@example.com", "Admin", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 2, "admin_1719000000003", 0 },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), "john.doe@example.com", "John Doe", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, "john-doe_1719000000004", 0 },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), "sarah.lee@example.com", "Sarah Lee", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, "sarah-lee_1719000000005", 0 }
                });

            migrationBuilder.InsertData(
                table: "CandidateProfiles",
                columns: new[] { "Id", "Bio", "CVUrl", "CurrentAmount", "PortfolioUrl" },
                values: new object[] { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Aspiring backend developer.", "https://example.com/cv-alice.pdf", 0, "https://portfolio.example.com/alice" });

            migrationBuilder.InsertData(
                table: "CoachProfiles",
                columns: new[] { "Id", "BankAccountNumber", "BankBinNumber", "Bio", "CurrentAmount", "ExperienceYears", "PortfolioUrl", "Status" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "", "", "Senior Backend Engineer with real interview experience", 0, 8, "https://portfolio.example.com/bob", 0 },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), "", "", "Fullstack Engineer previously at Uber", 0, 6, "https://portfolio.example.com/john", 0 },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), "", "", "Senior Frontend Engineer focusing on UI/UX interviews", 0, 7, "https://portfolio.example.com/sarah", 0 }
                });

            migrationBuilder.InsertData(
                table: "NotificationReceives",
                columns: new[] { "NotificationId", "ReceiverId" },
                values: new object[] { new Guid("0a1b2c3d-4e5f-4a6b-8c9d-0e1f2a3b4c20"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") });

            migrationBuilder.InsertData(
                table: "CandidateSkills",
                columns: new[] { "CandidateProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("02020202-0202-4202-8202-020202020202") },
                    { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") }
                });

            migrationBuilder.InsertData(
                table: "CoachAvailabilities",
                columns: new[] { "Id", "CoachId", "EndTime", "Focus", "StartTime", "Status", "TypeId" },
                values: new object[] { new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Utc), 0, new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Utc), 0, new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa") });

            migrationBuilder.InsertData(
                table: "CoachCompanies",
                columns: new[] { "CoachProfilesId", "CompaniesId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("11111111-1111-4111-8111-111111111111") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("44444444-4444-4444-8444-444444444444") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("33333333-3333-4333-8333-333333333333") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("66666666-6666-4666-8666-666666666666") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("88888888-8888-4888-8888-888888888888") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("22222222-2222-4222-8222-222222222222") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("77777777-7777-4777-8777-777777777777") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("99999999-9999-4999-8999-999999999999") }
                });

            migrationBuilder.InsertData(
                table: "CoachSkills",
                columns: new[] { "CoachProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("02020202-0202-4202-8202-020202020202") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("06060606-0606-4606-8606-060606060606") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("07070707-0707-4707-8707-070707070707") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("08080808-0808-4808-8808-080808080808") },
                    { new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("04040404-0404-4404-8404-040404040404") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("07070707-0707-4707-8707-070707070707") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("09090909-0909-4909-8909-090909090909") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                    { new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"), new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("0a0a0a0a-0a0a-4a0a-8a0a-0a0a0a0a0a0a") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("d3d3d3d3-d3d3-43d3-83d3-d3d3d3d3d3d3") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("e4e4e4e4-e4e4-44e4-84e4-e4e4e4e4e4e4") },
                    { new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"), new Guid("f5f5f5f5-f5f5-45f5-85f5-f5f5f5f5f5f5") }
                });

            migrationBuilder.InsertData(
                table: "InterviewRooms",
                columns: new[] { "Id", "CandidateId", "CoachId", "CurrentLanguage", "DurationMinutes", "LanguageCodes", "ProblemDescription", "ProblemShortName", "ScheduledTime", "Status", "TestCases", "VideoCallRoomUrl" },
                values: new object[] { new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), null, 60, null, null, null, new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Utc), 0, null, "https://meet.example/room1" });

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "Id", "AIAnalysis", "CandidateId", "CoachId", "Comments", "InterviewRoomId", "Rating" },
                values: new object[] { new Guid("9a0b1c2d-e3f4-4a5b-8c9d-0e1f2a3b4c10"), "{}", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), "Great answers and communication.", new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"), 5 });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkills_SkillsId",
                table: "CandidateSkills",
                column: "SkillsId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_CoachId",
                table: "CoachAvailabilities",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_TypeId",
                table: "CoachAvailabilities",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachCompanies_CompaniesId",
                table: "CoachCompanies",
                column: "CompaniesId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachSkills_SkillsId",
                table: "CoachSkills",
                column: "SkillsId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CandidateId",
                table: "Feedbacks",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CoachId",
                table: "Feedbacks",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_InterviewRoomId",
                table: "Feedbacks",
                column: "InterviewRoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_CoachAvailabilityId",
                table: "InterviewBookingTransaction",
                column: "CoachAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_UserId",
                table: "InterviewBookingTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CandidateId",
                table: "InterviewRooms",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CoachId",
                table: "InterviewRooms",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationReceives_ReceiverId",
                table: "NotificationReceives",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId_ExpiresAt",
                table: "PasswordResetTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SlugProfileUrl",
                table: "Users",
                column: "SlugProfileUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateSkills");

            migrationBuilder.DropTable(
                name: "CoachCompanies");

            migrationBuilder.DropTable(
                name: "CoachSkills");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "InterviewBookingTransaction");

            migrationBuilder.DropTable(
                name: "NotificationReceives");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "InterviewRooms");

            migrationBuilder.DropTable(
                name: "CoachAvailabilities");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "CandidateProfiles");

            migrationBuilder.DropTable(
                name: "CoachProfiles");

            migrationBuilder.DropTable(
                name: "InterviewTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
