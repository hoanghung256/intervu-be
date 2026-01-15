using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CoachAvailabilityChangeIsBookedToStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_CoachProfiles_CoachProfileId",
                table: "CoachAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewBookingTransaction_Users_UserId1",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropIndex(
                name: "IX_InterviewBookingTransaction_UserId1",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropIndex(
                name: "IX_CoachAvailabilities_CoachProfileId",
                table: "CoachAvailabilities");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "CoachProfileId",
                table: "CoachAvailabilities");

            migrationBuilder.DropColumn(
                name: "IsBooked",
                table: "CoachAvailabilities");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CoachAvailabilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                column: "Status",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "CoachAvailabilities");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "InterviewBookingTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoachProfileId",
                table: "CoachAvailabilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBooked",
                table: "CoachAvailabilities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                columns: new[] { "CoachProfileId", "IsBooked" },
                values: new object[] { null, false });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewBookingTransaction_UserId1",
                table: "InterviewBookingTransaction",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_CoachProfileId",
                table: "CoachAvailabilities",
                column: "CoachProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_CoachProfiles_CoachProfileId",
                table: "CoachAvailabilities",
                column: "CoachProfileId",
                principalTable: "CoachProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewBookingTransaction_Users_UserId1",
                table: "InterviewBookingTransaction",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
