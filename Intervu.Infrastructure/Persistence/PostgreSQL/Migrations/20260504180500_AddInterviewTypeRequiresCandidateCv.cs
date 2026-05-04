using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewTypeRequiresCandidateCv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresCandidateCv",
                table: "InterviewTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InterviewTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3f1c8b2-9d4e-4c7a-8f21-6b7e4d2c91aa"),
                column: "RequiresCandidateCv",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresCandidateCv",
                table: "InterviewTypes");
        }
    }
}
