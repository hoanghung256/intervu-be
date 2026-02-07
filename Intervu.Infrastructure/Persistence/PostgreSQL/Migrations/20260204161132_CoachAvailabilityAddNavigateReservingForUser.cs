using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CoachAvailabilityAddNavigateReservingForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
