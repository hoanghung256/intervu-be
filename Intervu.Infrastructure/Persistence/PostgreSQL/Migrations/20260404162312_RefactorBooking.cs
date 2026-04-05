using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InterviewRoundId",
                table: "CoachAvailabilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111111"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-1111-4a1a-8a1a-111111111112"),
                column: "InterviewRoundId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_InterviewRoundId",
                table: "CoachAvailabilities",
                column: "InterviewRoundId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_InterviewRounds_InterviewRoundId",
                table: "CoachAvailabilities",
                column: "InterviewRoundId",
                principalTable: "InterviewRounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_InterviewRounds_InterviewRoundId",
                table: "CoachAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_CoachAvailabilities_InterviewRoundId",
                table: "CoachAvailabilities");

            migrationBuilder.DropColumn(
                name: "InterviewRoundId",
                table: "CoachAvailabilities");
        }
    }
}
