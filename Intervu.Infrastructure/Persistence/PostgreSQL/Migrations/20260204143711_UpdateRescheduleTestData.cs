using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRescheduleTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                columns: new[] { "EndTime", "StartTime", "Status" },
                values: new object[] { new DateTime(2026, 2, 10, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 10, 9, 0, 0, 0, DateTimeKind.Utc), 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CoachAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"),
                columns: new[] { "EndTime", "StartTime", "Status" },
                values: new object[] { new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 1, 9, 0, 0, 0, DateTimeKind.Utc), 0 });
        }
    }
}
