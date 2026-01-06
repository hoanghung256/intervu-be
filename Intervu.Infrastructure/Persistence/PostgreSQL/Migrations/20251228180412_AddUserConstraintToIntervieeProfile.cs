using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserConstraintToIntervieeProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skills",
                table: "IntervieweeProfiles");

            migrationBuilder.CreateTable(
                name: "IntervieweeSkills",
                columns: table => new
                {
                    IntervieweeProfilesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntervieweeSkills", x => new { x.IntervieweeProfilesId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_IntervieweeSkills_IntervieweeProfiles_IntervieweeProfilesId",
                        column: x => x.IntervieweeProfilesId,
                        principalTable: "IntervieweeProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntervieweeSkills_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "IntervieweeSkills",
                columns: new[] { "IntervieweeProfilesId", "SkillsId" },
                values: new object[,]
                {
                    { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("02020202-0202-4202-8202-020202020202") },
                    { new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"), new Guid("b1b1b1b1-b1b1-41b1-81b1-b1b1b1b1b1b1") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntervieweeSkills_SkillsId",
                table: "IntervieweeSkills",
                column: "SkillsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntervieweeSkills");

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "IntervieweeProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "IntervieweeProfiles",
                keyColumn: "Id",
                keyValue: new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"),
                column: "Skills",
                value: null);
        }
    }
}
