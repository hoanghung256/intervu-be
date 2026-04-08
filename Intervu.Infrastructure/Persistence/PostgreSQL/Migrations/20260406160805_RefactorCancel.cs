using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCancel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewBookingTransaction_CoachAvailabilities_CoachAvailabilityId",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropIndex(
                name: "IX_InterviewBookingTransaction_CoachAvailabilityId",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "BookedDurationMinutes",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "BookedStartTime",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "CoachAvailabilityId",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "CoachId",
                table: "InterviewBookingTransaction");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposedEndTime",
                table: "InterviewRescheduleRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposedStartTime",
                table: "InterviewRescheduleRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProposedEndTime",
                table: "InterviewRescheduleRequests");

            migrationBuilder.DropColumn(
                name: "ProposedStartTime",
                table: "InterviewRescheduleRequests");

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
                name: "CoachAvailabilityId",
                table: "InterviewBookingTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoachId",
                table: "InterviewBookingTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00"),
                columns: new[] { "BookedDurationMinutes", "BookedStartTime", "CoachAvailabilityId", "CoachId" },
                values: new object[] { null, null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"), null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"),
                columns: new[] { "BookedDurationMinutes", "BookedStartTime", "CoachAvailabilityId", "CoachId" },
                values: new object[] { null, null, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99"),
                columns: new[] { "BookedDurationMinutes", "BookedStartTime", "CoachAvailabilityId", "CoachId" },
                values: new object[] { null, null, new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"), null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"),
                columns: new[] { "BookedDurationMinutes", "BookedStartTime", "CoachAvailabilityId", "CoachId" },
                values: new object[] { null, null, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), null });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_CoachAvailabilityId",
                table: "InterviewBookingTransaction",
                column: "CoachAvailabilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewBookingTransaction_CoachAvailabilities_CoachAvailabilityId",
                table: "InterviewBookingTransaction",
                column: "CoachAvailabilityId",
                principalTable: "CoachAvailabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
