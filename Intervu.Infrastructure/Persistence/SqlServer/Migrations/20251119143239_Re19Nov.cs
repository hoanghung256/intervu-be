using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Re19Nov : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    ProfilePicture = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewBookingTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    InterviewerAvailabilityId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewBookingTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewBookingTransaction_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntervieweeProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CVUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PortfolioUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentAmount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntervieweeProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntervieweeProfiles_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PortfolioUrl = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CurrentAmount = table.Column<int>(type: "int", nullable: true),
                    ExperienceYears = table.Column<int>(type: "int", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankBinNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewerProfiles_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationReceives",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false)
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
                name: "InterviewerAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewerId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBooked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewerAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewerAvailabilities_InterviewerProfiles_InterviewerId",
                        column: x => x.InterviewerId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewerCompanies",
                columns: table => new
                {
                    InterviewerProfilesId = table.Column<int>(type: "int", nullable: false),
                    CompaniesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewerCompanies", x => new { x.InterviewerProfilesId, x.CompaniesId });
                    table.ForeignKey(
                        name: "FK_InterviewerCompanies_Companies_CompaniesId",
                        column: x => x.CompaniesId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewerCompanies_InterviewerProfiles_InterviewerProfilesId",
                        column: x => x.InterviewerProfilesId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewerSkills",
                columns: table => new
                {
                    InterviewerProfilesId = table.Column<int>(type: "int", nullable: false),
                    SkillsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewerSkills", x => new { x.InterviewerProfilesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_InterviewerSkills_InterviewerProfiles_InterviewerProfilesId",
                        column: x => x.InterviewerProfilesId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewerSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: true),
                    InterviewerId = table.Column<int>(type: "int", nullable: true),
                    ScheduledTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    VideoCallRoomUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CurrentLanguage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LanguageCodes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProblemDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProblemShortName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TestCases = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_IntervieweeProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "IntervieweeProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewRooms_InterviewerProfiles_InterviewerId",
                        column: x => x.InterviewerId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewerId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    InterviewRoomId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AIAnalysis = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_InterviewRooms_InterviewRoomId",
                        column: x => x.InterviewRoomId,
                        principalTable: "InterviewRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_IntervieweeProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "IntervieweeProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_InterviewerProfiles_InterviewerId",
                        column: x => x.InterviewerId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "LogoPath", "Name", "Website" },
                values: new object[,]
                {
                    { 1, "logos/google.png", "Google", "https://google.com" },
                    { 2, "logos/meta.png", "Meta", "https://meta.com" },
                    { 3, "logos/amazon.png", "Amazon", "https://amazon.com" },
                    { 4, "logos/microsoft.png", "Microsoft", "https://microsoft.com" },
                    { 5, "logos/netflix.png", "Netflix", "https://netflix.com" },
                    { 6, "logos/tiktok.png", "TikTok", "https://tiktok.com" },
                    { 7, "logos/apple.png", "Apple", "https://apple.com" },
                    { 8, "logos/uber.png", "Uber", "https://uber.com" },
                    { 9, "logos/spotify.png", "Spotify", "https://spotify.com" },
                    { 10, "logos/stripe.png", "Stripe", "https://stripe.com" }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "CreatedAt", "Message", "Title" },
                values: new object[] { 1, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Welcome to Intervu platform", "Welcome" });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, null, "C#" },
                    { 2, null, "Java" },
                    { 3, null, "JavaScript" },
                    { 4, null, "TypeScript" },
                    { 5, null, "React" },
                    { 6, null, "Node.js" },
                    { 7, null, "SQL" },
                    { 8, null, "MongoDB" },
                    { 9, null, "AWS" },
                    { 10, null, "Azure" },
                    { 11, null, "System Design" },
                    { 12, null, "Microservices" },
                    { 13, null, "Docker" },
                    { 14, null, "Kubernetes" },
                    { 15, null, "Machine Learning" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Password", "ProfilePicture", "Role", "Status" },
                values: new object[,]
                {
                    { 1, "alice@example.com", "Alice Student", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 0, 0 },
                    { 2, "bob@example.com", "Bob Interviewer", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, 0 },
                    { 3, "admin@example.com", "Admin", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 2, 0 },
                    { 5, "john.doe@example.com", "John Doe", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, 0 },
                    { 6, "sarah.lee@example.com", "Sarah Lee", "10000.QdMM6/umqXH7gdmWhCSo6A==.vfa//iQ7atLzzEXuLQLrQa2+MkrJeouJdN/Bxs81Blo=", null, 1, 0 }
                });

            migrationBuilder.InsertData(
                table: "InterviewBookingTransaction",
                columns: new[] { "Id", "Amount", "InterviewerAvailabilityId", "Status", "Type", "UserId" },
                values: new object[,]
                {
                    { 1, 1000, 1, 1, 0, 1 },
                    { 2, 500, 1, 1, 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "IntervieweeProfiles",
                columns: new[] { "Id", "Bio", "CVUrl", "CurrentAmount", "PortfolioUrl", "Skills" },
                values: new object[] { 1, "Aspiring backend developer.", "https://example.com/cv-alice.pdf", 0, "https://portfolio.example.com/alice", "[C#, SQL]" });

            migrationBuilder.InsertData(
                table: "InterviewerProfiles",
                columns: new[] { "Id", "BankAccountNumber", "BankBinNumber", "Bio", "CurrentAmount", "ExperienceYears", "PortfolioUrl", "Status" },
                values: new object[,]
                {
                    { 2, "", "", "Senior Backend Engineer with real interview experience", 0, 8, "https://portfolio.example.com/bob", 0 },
                    { 5, "", "", "Fullstack Engineer previously at Uber", 0, 6, "https://portfolio.example.com/john", 0 },
                    { 6, "", "", "Senior Frontend Engineer focusing on UI/UX interviews", 0, 7, "https://portfolio.example.com/sarah", 0 }
                });

            migrationBuilder.InsertData(
                table: "NotificationReceives",
                columns: new[] { "NotificationId", "ReceiverId" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                table: "InterviewRooms",
                columns: new[] { "Id", "CurrentLanguage", "DurationMinutes", "InterviewerId", "LanguageCodes", "ProblemDescription", "ProblemShortName", "ScheduledTime", "Status", "StudentId", "TestCases", "VideoCallRoomUrl" },
                values: new object[] { 1, null, 60, 2, null, null, null, new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), 0, 1, null, "https://meet.example/room1" });

            migrationBuilder.InsertData(
                table: "InterviewerAvailabilities",
                columns: new[] { "Id", "EndTime", "InterviewerId", "IsBooked", "StartTime" },
                values: new object[] { 1, new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, false, new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "InterviewerCompanies",
                columns: new[] { "CompaniesId", "InterviewerProfilesId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 4, 2 },
                    { 10, 2 },
                    { 3, 5 },
                    { 6, 5 },
                    { 8, 5 },
                    { 2, 6 },
                    { 7, 6 },
                    { 9, 6 }
                });

            migrationBuilder.InsertData(
                table: "InterviewerSkills",
                columns: new[] { "InterviewerProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 2, 7 },
                    { 2, 11 },
                    { 2, 12 },
                    { 2, 13 },
                    { 5, 3 },
                    { 5, 4 },
                    { 5, 9 },
                    { 5, 12 },
                    { 5, 14 },
                    { 6, 3 },
                    { 6, 4 },
                    { 6, 5 },
                    { 6, 15 }
                });

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "Id", "AIAnalysis", "Comments", "InterviewRoomId", "InterviewerId", "Rating", "StudentId" },
                values: new object[] { 1, "{}", "Great answers and communication.", 1, 2, 5, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_InterviewerId",
                table: "Feedbacks",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_InterviewRoomId",
                table: "Feedbacks",
                column: "InterviewRoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_StudentId",
                table: "Feedbacks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_UserId",
                table: "InterviewBookingTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerAvailabilities_InterviewerId",
                table: "InterviewerAvailabilities",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerCompanies_CompaniesId",
                table: "InterviewerCompanies",
                column: "CompaniesId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerSkills_SkillsId",
                table: "InterviewerSkills",
                column: "SkillsId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_InterviewerId",
                table: "InterviewRooms",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_StudentId",
                table: "InterviewRooms",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationReceives_ReceiverId",
                table: "NotificationReceives",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "InterviewBookingTransaction");

            migrationBuilder.DropTable(
                name: "InterviewerAvailabilities");

            migrationBuilder.DropTable(
                name: "InterviewerCompanies");

            migrationBuilder.DropTable(
                name: "InterviewerSkills");

            migrationBuilder.DropTable(
                name: "NotificationReceives");

            migrationBuilder.DropTable(
                name: "InterviewRooms");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "IntervieweeProfiles");

            migrationBuilder.DropTable(
                name: "InterviewerProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
