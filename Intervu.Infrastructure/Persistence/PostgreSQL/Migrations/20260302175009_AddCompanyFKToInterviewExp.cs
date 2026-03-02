using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyFKToInterviewExp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewExperiences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
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
                        name: "FK_InterviewExperiences_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Vote = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsAnswer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "LogoPath", "Name", "Website" },
                values: new object[] { new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), "logos/shopee.png", "Shopee", "https://shopee.com" });

            migrationBuilder.InsertData(
                table: "InterviewExperiences",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "CreatedBy", "InterviewProcess", "IsInterestedInContact", "LastRoundCompleted", "Level", "Role", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), new Guid("11111111-1111-4111-8111-111111111111"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Phone screen → 2 technical rounds → system design → behavioral", true, "Onsite", 3, "Software Engineer", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e"), new Guid("22222222-2222-4222-8222-222222222222"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), "Online assessment → coding interview → system design", false, "System Design", 2, "Frontend Developer", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f"), new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), "CV screening → HR call → technical interview with coding challenge", true, "Technical", 1, "Backend Engineer", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33") }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Answer", "Content", "CreatedAt", "InterviewExperienceId", "QuestionType" },
                values: new object[,]
                {
                    { new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), "Use sliding window with a HashSet. O(n) time.", "Find the longest substring without repeating characters.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), "Algorithm" },
                    { new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), "Use consistent hashing, a KV store, and a redirect service.", "Design a URL shortener like bit.ly.", new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d"), "System Design" },
                    { new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), "== coerces types, === does not.", "Explain the difference between == and ===.", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("b2c3d4e5-f6a1-4b2c-9d3e-4f5a6b7c8d9e"), "JavaScript" }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c1c1c1c1-1111-4c11-8c11-c1c1c1c1c111"), "Use sliding window with a HashSet. O(n) time.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 10 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c2c2c2c2-2222-4c22-8c22-c2c2c2c2c222"), "You can also use a dictionary to map each character to its latest index for O(n) in a single pass.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("d4e5f6a1-b2c3-4d4e-1f5a-6b7c8d9e0f1a"), new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 5 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c3c3c3c3-3333-4c33-8c33-c3c3c3c3c333"), "Use consistent hashing, a KV store, and a redirect service.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 8 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c4c4c4c4-4444-4c44-8c44-c4c4c4c4c444"), "Don't forget rate limiting and analytics counters when discussing the redirect service design.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("e5f6a1b2-c3d4-4e5f-2a6b-7c8d9e0f1a2b"), new DateTime(2026, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 3 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c5c5c5c5-5555-4c55-8c55-c5c5c5c5c555"), "== coerces types, === does not.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 7 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c6c6c6c6-6666-4c66-8c66-c6c6c6c6c666"), "Always prefer === in modern JS/TS to avoid implicit coercion bugs. null == undefined is true but null === undefined is false.", new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("f6a1b2c3-d4e5-4f6a-3b7c-8d9e0f1a2b3c"), new DateTime(2026, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"), 4 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Answer", "Content", "CreatedAt", "InterviewExperienceId", "QuestionType" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.", "Reverse a linked list.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("c3d4e5f6-a1b2-4c3d-0e4f-5a6b7c8d9e0f"), "Algorithm" });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "IsAnswer", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c7c7c7c7-7777-4c77-8c77-c7c7c7c7c777"), "Iterative approach using prev, curr, next pointers. O(n) time O(1) space.", new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"), 9 });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "Content", "CreateBy", "CreatedAt", "QuestionId", "UpdateAt", "UpdateBy", "Vote" },
                values: new object[] { new Guid("c8c8c8c8-8888-4c88-8c88-c8c8c8c8c888"), "Recursive solution is cleaner to write but costs O(n) stack space. Interviewers often ask you to do both.", new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1b2c3d4-e5f6-4a7b-4c8d-9e0f1a2b3c4d"), new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), 6 });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_QuestionId",
                table: "Comments",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewExperiences_CompanyId",
                table: "InterviewExperiences",
                column: "CompanyId");

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
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "InterviewExperiences");

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"));
        }
    }
}
