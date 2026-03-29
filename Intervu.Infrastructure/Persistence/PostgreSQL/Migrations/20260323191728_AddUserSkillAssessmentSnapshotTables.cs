using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSkillAssessmentSnapshotTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAssessmentAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Skill = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: true),
                    SelectedLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SfiaLevel = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssessmentAnswers", x => x.Id);
                    table.CheckConstraint("CK_UserAssessmentAnswers_SelectedLevel", "\"SelectedLevel\" IN ('None','Basic','Intermediate','Advanced')");
                    table.CheckConstraint("CK_UserAssessmentAnswers_SfiaLevel", "\"SfiaLevel\" IN (0,2,3,5)");
                    table.ForeignKey(
                        name: "FK_UserAssessmentAnswers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSkillAssessmentSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetJson = table.Column<string>(type: "jsonb", nullable: false),
                    CurrentJson = table.Column<string>(type: "jsonb", nullable: false),
                    GapJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkillAssessmentSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSkillAssessmentSnapshots_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAnswers_AssessmentId",
                table: "UserAssessmentAnswers",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAnswers_Skill",
                table: "UserAssessmentAnswers",
                column: "Skill");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAnswers_UserId",
                table: "UserAssessmentAnswers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkillAssessmentSnapshots_UserId",
                table: "UserSkillAssessmentSnapshots",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAssessmentAnswers");

            migrationBuilder.DropTable(
                name: "UserSkillAssessmentSnapshots");
        }
    }
}
