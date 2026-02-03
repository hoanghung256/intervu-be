using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CoachAvailabilityAddReservingForUserIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_CoachProfiles_CoachId",
                table: "CoachAvailabilities");

            migrationBuilder.AlterColumn<int>(
                name: "OrderCode",
                table: "InterviewBookingTransaction",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "ReservingForUserId",
                table: "CoachAvailabilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                column: "ReservingForUserId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_CoachProfiles_CoachId",
                table: "CoachAvailabilities",
                column: "CoachId",
                principalTable: "CoachProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                table: "CoachAvailabilities",
                column: "ReservingForUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_CoachProfiles_CoachId",
                table: "CoachAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_Users_ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_CoachAvailabilities_ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.DropColumn(
                name: "ReservingForUserId",
                table: "CoachAvailabilities");

            migrationBuilder.AlterColumn<int>(
                name: "OrderCode",
                table: "InterviewBookingTransaction",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_CoachProfiles_CoachId",
                table: "CoachAvailabilities",
                column: "CoachId",
                principalTable: "CoachProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
