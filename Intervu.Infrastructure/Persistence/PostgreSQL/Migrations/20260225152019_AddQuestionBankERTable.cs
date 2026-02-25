using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBankERTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewExperiences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Role = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: true),
                    LastRoundCompleted = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InterviewProcess = table.Column<string>(type: "text", nullable: false),
                    IsInterestedInContact = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewExperiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewExperiences_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewExperienceId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_InterviewExperiences_InterviewExperienceId",
                        column: x => x.InterviewExperienceId,
                        principalTable: "InterviewExperiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "InterviewExperiences",
                columns: new[] { "Id", "CompanyName", "CreatedAt", "CreatedBy", "InterviewProcess", "IsInterestedInContact", "LastRoundCompleted", "Level", "Role", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), "Google", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Phone screen → 2 technical rounds → system design → behavioral", true, "Onsite", 3, "Software Engineer", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e"), "Meta", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Online assessment → coding interview → system design", false, "System Design", 2, "Frontend Developer", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f"), "Shopee", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), "CV screening → HR call → technical interview with coding challenge", true, "Technical", 1, "Backend Engineer", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33") }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Answer", "Content", "CreatedAt", "InterviewExperienceId", "QuestionType" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.", "Reverse a linked list.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f"), "Algorithm" },
                    { new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), "Use sliding window with a HashSet. O(n) time.", "Find the longest substring without repeating characters.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), "Algorithm" },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), "Use consistent hashing, a KV store, and a redirect service.", "Design a URL shortener like bit.ly.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), "System Design" },
                    { new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), "== coerces types, === does not.", "Explain the difference between == and ===.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e"), "JavaScript" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewExperiences_CreatedBy",
                table: "InterviewExperiences",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_InterviewExperienceId",
                table: "Questions",
                column: "InterviewExperienceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "InterviewExperiences");
        }
    }
}
