using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Intervu.Infrastructure.Persistence.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCodeColumnForInterviewBookingTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"));

            migrationBuilder.DeleteData(
                table: "InterviewBookingTransaction",
                keyColumn: "Id",
                keyValue: new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"));

            migrationBuilder.AddColumn<int>(
                name: "OrderCode",
                table: "InterviewBookingTransaction",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "InterviewBookingTransaction");

            migrationBuilder.InsertData(
                table: "InterviewBookingTransaction",
                columns: new[] { "Id", "Amount", "CoachAvailabilityId", "Status", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("7e8f9a0b-c1d2-4e3f-8a9b-0c1d2e3f4a88"), 1000, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), 1, 0, new Guid("0d0b8b1e-2e2c-43e2-9d8e-7d2f7a2a1a11") },
                    { new Guid("8f9a0b1c-d2e3-4f5a-9b0c-1d2e3f4a5b99"), 500, new Guid("6d7e8f9a-b8a9-4c3d-8f9e-6d5c4b3a2a77"), 1, 1, new Guid("1e9f9d3b-5b4c-4f1d-9f3a-8b8c3e2d4c22") }
                });
        }
    }
}
