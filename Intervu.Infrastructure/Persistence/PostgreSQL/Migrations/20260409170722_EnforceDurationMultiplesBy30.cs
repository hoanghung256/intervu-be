using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class EnforceDurationMultiplesBy30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                     UPDATE "InterviewTypes"
                     SET "SuggestedDurationMinutes" = LEAST(300, GREATEST(30, (("SuggestedDurationMinutes" + 29) / 30) * 30))
                     WHERE "SuggestedDurationMinutes" % 30 <> 0
                         OR "SuggestedDurationMinutes" < 15
                         OR "SuggestedDurationMinutes" > 300;
                """);

            migrationBuilder.Sql(
                """
                     UPDATE "CoachInterviewServices"
                     SET "DurationMinutes" = LEAST(300, GREATEST(30, (("DurationMinutes" + 29) / 30) * 30))
                     WHERE "DurationMinutes" % 30 <> 0
                         OR "DurationMinutes" < 15
                         OR "DurationMinutes" > 300;
                """);

            migrationBuilder.UpdateData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cd"),
                column: "DurationMinutes",
                value: 90);

            migrationBuilder.UpdateData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cf"),
                column: "DurationMinutes",
                value: 60);

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                column: "SuggestedDurationMinutes",
                value: 60);

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                column: "SuggestedDurationMinutes",
                value: 90);

            migrationBuilder.AddCheckConstraint(
                name: "CK_InterviewTypes_SuggestedDurationMinutes_MultipleOf30",
                table: "InterviewTypes",
                sql: "\"SuggestedDurationMinutes\" >= 15 AND \"SuggestedDurationMinutes\" <= 300 AND \"SuggestedDurationMinutes\" % 30 = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoachInterviewServices_DurationMinutes_MultipleOf30",
                table: "CoachInterviewServices",
                sql: "\"DurationMinutes\" >= 15 AND \"DurationMinutes\" <= 300 AND \"DurationMinutes\" % 30 = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_InterviewTypes_SuggestedDurationMinutes_MultipleOf30",
                table: "InterviewTypes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoachInterviewServices_DurationMinutes_MultipleOf30",
                table: "CoachInterviewServices");

            migrationBuilder.UpdateData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cd"),
                column: "DurationMinutes",
                value: 75);

            migrationBuilder.UpdateData(
                table: "CoachInterviewServices",
                keyColumn: "Id",
                keyValue: new Guid("019d1467-d415-79f8-9bdc-5bb25a0b25cf"),
                column: "DurationMinutes",
                value: 45);

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("5c9e2a14-73bb-4b61-b7e2-91a8f42d3c6e"),
                column: "SuggestedDurationMinutes",
                value: 45);

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("f14a7c6d-88b2-4d55-a9fd-2b4e73c91a08"),
                column: "SuggestedDurationMinutes",
                value: 75);
        }
    }
}
