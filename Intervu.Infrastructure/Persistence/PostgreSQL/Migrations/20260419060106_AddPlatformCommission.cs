using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommissionAmount",
                table: "InterviewBookingTransaction",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                table: "InterviewBookingTransaction",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrossAmount",
                table: "InterviewBookingTransaction",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a00"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a11"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a22"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a99"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("b1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5e"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("c1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5f"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("f1f1f1f1-9999-4a1a-8a1a-999999999999"),
                columns: new[] { "CommissionAmount", "CommissionRate", "GrossAmount" },
                values: new object[] { null, null, null });

            migrationBuilder.InsertData(
                table: "PlatformSettings",
                columns: new[] { "Id", "CommissionRate", "CreatedAt", "UpdatedAt" },
                values: new object[] { new Guid("019d9aa0-0000-7000-8000-000000000001"), 0.30m, new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Utc), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformSettings");

            migrationBuilder.DropColumn(
                name: "CommissionAmount",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "InterviewBookingTransaction");

            migrationBuilder.DropColumn(
                name: "GrossAmount",
                table: "InterviewBookingTransaction");
        }
    }
}
