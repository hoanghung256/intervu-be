using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCoachWithType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TypeId",
                table: "CoachAvailabilities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                column: "TypeId",
                value: new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"));

            migrationBuilder.CreateIndex(
                name: "IX_CoachAvailabilities_TypeId",
                table: "CoachAvailabilities",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoachAvailabilities_InterviewTypes_TypeId",
                table: "CoachAvailabilities",
                column: "TypeId",
                principalTable: "InterviewTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoachAvailabilities_InterviewTypes_TypeId",
                table: "CoachAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_CoachAvailabilities_TypeId",
                table: "CoachAvailabilities");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "CoachAvailabilities");
        }
    }
}
