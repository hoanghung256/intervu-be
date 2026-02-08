using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRescheduleRequestToInterviewRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRescheduleRequests_InterviewBookingTransaction_InterviewBookingTransactionId",
                table: "InterviewRescheduleRequests");

            migrationBuilder.RenameColumn(
                name: "InterviewBookingTransactionId",
                table: "InterviewRescheduleRequests",
                newName: "InterviewRoomId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewRescheduleRequests_InterviewBookingTransactionId_S~",
                table: "InterviewRescheduleRequests",
                newName: "IX_InterviewRescheduleRequests_InterviewRoomId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewRescheduleRequests_InterviewBookingTransactionId",
                table: "InterviewRescheduleRequests",
                newName: "IX_InterviewRescheduleRequests_InterviewRoomId");

            migrationBuilder.AddColumn<int>(
                name: "RescheduleAttemptCount",
                table: "InterviewRooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                column: "RescheduleAttemptCount",
                value: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRescheduleRequests_InterviewRooms_InterviewRoomId",
                table: "InterviewRescheduleRequests",
                column: "InterviewRoomId",
                principalTable: "InterviewRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRescheduleRequests_InterviewRooms_InterviewRoomId",
                table: "InterviewRescheduleRequests");

            migrationBuilder.DropColumn(
                name: "RescheduleAttemptCount",
                table: "InterviewRooms");

            migrationBuilder.RenameColumn(
                name: "InterviewRoomId",
                table: "InterviewRescheduleRequests",
                newName: "InterviewBookingTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewRescheduleRequests_InterviewRoomId_Status",
                table: "InterviewRescheduleRequests",
                newName: "IX_InterviewRescheduleRequests_InterviewBookingTransactionId_S~");

            migrationBuilder.RenameIndex(
                name: "IX_InterviewRescheduleRequests_InterviewRoomId",
                table: "InterviewRescheduleRequests",
                newName: "IX_InterviewRescheduleRequests_InterviewBookingTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRescheduleRequests_InterviewBookingTransaction_InterviewBookingTransactionId",
                table: "InterviewRescheduleRequests",
                column: "InterviewBookingTransactionId",
                principalTable: "InterviewBookingTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
