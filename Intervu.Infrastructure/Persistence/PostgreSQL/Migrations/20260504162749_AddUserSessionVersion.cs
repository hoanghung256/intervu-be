using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSessionVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionVersion",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11"),
                column: "SessionVersion",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22"),
                column: "SessionVersion",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("2f8c7a6b-6d5e-4e2f-8c7a-9d6e5c4b3a33"),
                column: "SessionVersion",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3a7b6c5d-7e6f-4d3c-9b8a-7c6d5e4f3b44"),
                column: "SessionVersion",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4b6c5d7e-8f7a-4c3d-9e8b-6d5c4f3e2a55"),
                column: "SessionVersion",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionVersion",
                table: "Users");
        }
    }
}
