using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnInterviewBookingTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookedDurationMinutes",
                table: "InterviewBookingTransaction",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BookedStartTime",
                table: "InterviewBookingTransaction",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoachId",
                table: "InterviewBookingTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"),
                columns: new[] { "BookedDurationMinutes", "BookedStartTime", "CoachId" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"),
                columns: new[] { "BookedDurationMinutes", "BookedStartTime", "CoachId" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookedDurationMinutes",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "BookedStartTime",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "CoachId",
                table: "InterviewBookingTransaction");
        }
    }
}
