using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewTypes");
        }
    }
}
