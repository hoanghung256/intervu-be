using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentAvailabilityToInterviewRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentAvailabilityId",
                table: "InterviewRooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InterviewRooms",
                keyColumn: "Id",
                keyValue: new Guid("5c5d6e7f-9a8b-4d3c-8e9b-7c6d5e4f3a66"),
                column: "CurrentAvailabilityId",
                value: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"));

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRooms_CurrentAvailabilityId",
                table: "InterviewRooms",
                column: "CurrentAvailabilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewRooms_CoachAvailabilities_CurrentAvailabilityId",
                table: "InterviewRooms",
                column: "CurrentAvailabilityId",
                principalTable: "CoachAvailabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewRooms_CoachAvailabilities_CurrentAvailabilityId",
                table: "InterviewRooms");

            migrationBuilder.DropIndex(
                name: "IX_InterviewRooms_CurrentAvailabilityId",
                table: "InterviewRooms");

            migrationBuilder.DropColumn(
                name: "CurrentAvailabilityId",
                table: "InterviewRooms");
        }
    }
}
