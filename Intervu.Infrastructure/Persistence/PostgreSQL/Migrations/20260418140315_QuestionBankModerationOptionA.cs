using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class QuestionBankModerationOptionA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Questions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActionTaken",
                table: "QuestionReports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<System.DateTime>(
                name: "ResolvedAt",
                table: "QuestionReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<System.Guid>(
                name: "ResolvedBy",
                table: "QuestionReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNote",
                table: "QuestionReports",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_IsHidden",
                table: "Questions",
                column: "IsHidden");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionReports_ResolvedAt",
                table: "QuestionReports",
                column: "ResolvedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionReports_ResolvedBy",
                table: "QuestionReports",
                column: "ResolvedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionReports_ResolvedBy",
                table: "QuestionReports");

            migrationBuilder.DropIndex(
                name: "IX_QuestionReports_ResolvedAt",
                table: "QuestionReports");

            migrationBuilder.DropIndex(
                name: "IX_Questions_IsHidden",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ResolutionNote",
                table: "QuestionReports");

            migrationBuilder.DropColumn(
                name: "ResolvedBy",
                table: "QuestionReports");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "QuestionReports");

            migrationBuilder.DropColumn(
                name: "ActionTaken",
                table: "QuestionReports");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Questions");
        }
    }
}
