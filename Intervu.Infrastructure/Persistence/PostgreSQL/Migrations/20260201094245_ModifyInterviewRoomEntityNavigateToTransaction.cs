using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class ModifyInterviewRoomEntityNavigateToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "InterviewRooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                column: "TransactionId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_TransactionId",
                table: "InterviewRooms",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRooms_InterviewBookingTransaction_TransactionId",
                table: "InterviewRooms",
                column: "TransactionId",
                principalTable: "InterviewBookingTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRooms_InterviewBookingTransaction_TransactionId",
                table: "InterviewRooms");

            migrationBuilder.DropIndex(
                name: "IX_InterviewRooms_TransactionId",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "InterviewRooms");
        }
    }
}
