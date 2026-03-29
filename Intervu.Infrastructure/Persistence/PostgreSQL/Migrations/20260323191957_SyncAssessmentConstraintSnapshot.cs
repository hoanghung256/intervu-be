using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class SyncAssessmentConstraintSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAssessmentAnswers_SelectedLevel",
                table: "UserAssessmentAnswers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAssessmentAnswers_SfiaLevel",
                table: "UserAssessmentAnswers");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAssessmentAnswers_SelectedLevel",
                table: "UserAssessmentAnswers",
                sql: "\"SelectedLevel\" IN ('None','Basic','Intermediate','Advanced')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAssessmentAnswers_SfiaLevel",
                table: "UserAssessmentAnswers",
                sql: "\"SfiaLevel\" IN (0,2,3,5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAssessmentAnswers_SelectedLevel",
                table: "UserAssessmentAnswers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAssessmentAnswers_SfiaLevel",
                table: "UserAssessmentAnswers");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAssessmentAnswers_SelectedLevel",
                table: "UserAssessmentAnswers",
                sql: "SelectedLevel IN ('None','Basic','Intermediate','Advanced')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAssessmentAnswers_SfiaLevel",
                table: "UserAssessmentAnswers",
                sql: "SfiaLevel IN (0,2,3,5)");
        }
    }
}
