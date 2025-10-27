using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IntervieweeProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CVUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PortfolioUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    CVUrl = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    PortfolioUrl = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Specializations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgrammingLanguages = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrentAmount = table.Column<int>(type: "int", nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewerId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AIAnalysis = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
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
                name: "InterviewRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    InterviewerId = table.Column<int>(type: "int", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    VideoCallRoomUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
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
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewRoomId = table.Column<int>(type: "int", nullable: false),
                    IntervieweeId = table.Column<int>(type: "int", nullable: false),
                    InterviewerId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_InterviewRooms_InterviewRoomId",
                        column: x => x.InterviewRoomId,
                        principalTable: "InterviewRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_IntervieweeProfiles_IntervieweeId",
                        column: x => x.IntervieweeId,
                        principalTable: "IntervieweeProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_InterviewerProfiles_InterviewerId",
                        column: x => x.InterviewerId,
                        principalTable: "InterviewerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "CreatedAt", "Message", "Title" },
                values: new object[] { 1, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Welcome to Intervu platform", "Welcome" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Password", "ProfilePicture", "Role", "Status" },
                values: new object[,]
                {
                    { 1, "alice@example.com", "Alice Student", "hashedpassword", null, 0, 0 },
                    { 2, "bob@example.com", "Bob Interviewer", "hashedpassword", null, 1, 0 },
                    { 3, "admin@example.com", "Admin", "hashedpassword", null, 2, 0 }
                });

            migrationBuilder.InsertData(
                table: "IntervieweeProfiles",
                columns: new[] { "Id", "Bio", "CVUrl", "CurrentAmount", "PortfolioUrl", "Skills" },
                values: new object[] { 1, "Aspiring backend developer.", "https://example.com/cv-alice.pdf", 0, "https://portfolio.example.com/alice", "[C#, SQL]" });

            migrationBuilder.InsertData(
                table: "InterviewerProfiles",
                columns: new[] { "Id", "Bio", "CVUrl", "Company", "CurrentAmount", "ExperienceYears", "IsVerified", "PortfolioUrl", "ProgrammingLanguages", "Specializations" },
                values: new object[] { 2, "Senior software engineer", "https://example.com/cv-bob.pdf", "Tech Co", 0, 8, true, "https://portfolio.example.com/bob", "C#, JavaScript", "Backend, System Design" });

            migrationBuilder.InsertData(
                table: "NotificationReceives",
                columns: new[] { "NotificationId", "ReceiverId" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "Id", "AIAnalysis", "Comments", "InterviewerId", "Rating", "StudentId" },
                values: new object[] { 1, "{}", "Great answers and communication.", 2, 5, 1 });

            migrationBuilder.InsertData(
                table: "InterviewRooms",
                columns: new[] { "Id", "DurationMinutes", "InterviewerId", "ScheduledTime", "Status", "StudentId", "VideoCallRoomUrl" },
                values: new object[] { 1, 60, 2, new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Unspecified), 0, 1, "https://meet.example/room1" });

            migrationBuilder.InsertData(
                table: "InterviewerAvailabilities",
                columns: new[] { "Id", "EndTime", "InterviewerId", "IsBooked", "StartTime" },
                values: new object[] { 1, new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 2, false, new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "Amount", "InterviewRoomId", "IntervieweeId", "InterviewerId", "PaymentMethod", "Status", "TransactionDate" },
                values: new object[] { 1, 50.00m, 1, 1, 2, "Card", 0, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_InterviewerId",
                table: "Feedbacks",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_StudentId",
                table: "Feedbacks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewerAvailabilities_InterviewerId",
                table: "InterviewerAvailabilities",
                column: "InterviewerId");

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
                name: "IX_Payments_IntervieweeId",
                table: "Payments",
                column: "IntervieweeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InterviewerId",
                table: "Payments",
                column: "InterviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InterviewRoomId",
                table: "Payments",
                column: "InterviewRoomId");

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
                name: "InterviewerAvailabilities");

            migrationBuilder.DropTable(
                name: "NotificationReceives");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "InterviewRooms");

            migrationBuilder.DropTable(
                name: "IntervieweeProfiles");

            migrationBuilder.DropTable(
                name: "InterviewerProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
